/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Localization;
using OneScript.Native.Runtime;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    public class MethodCompiler : ExpressionTreeGeneratorBase
    {
        private readonly BslMethodInfo _method;
        private readonly List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        private readonly StatementBlocksWriter _blocks = new StatementBlocksWriter();
        private readonly Stack<Expression> _statementBuildParts = new Stack<Expression>();
        private BslParameterInfo[] _declaredParameters;
        
        private BinaryOperationCompiler _binaryOperationCompiler = new BinaryOperationCompiler();
        
        public MethodCompiler(BslWalkerContext walkContext, BslMethodInfo method) : base(walkContext)
        {
            _method = method;
        }

        public void CompileMethod(MethodNode methodNode)
        {
            _localVariables.AddRange(
                _method.GetParameters()
                    .Select(x => Expression.Parameter(typeof(BslValue), x.Name)));
            
            CompileFragment(methodNode, x=>VisitMethodBody((MethodNode)x));
        }
        
        public void CompileModuleBody(BslMethodInfo method, BslSyntaxNode moduleBodyNode)
        {
            CompileFragment(moduleBodyNode, Visit);
        }
        
        private class InternalFlowInterruptException : Exception
        {
        }
        
        private void CompileFragment(BslSyntaxNode node, Action<BslSyntaxNode> visitor)
        {
            _blocks.EnterBlock(new JumpInformationRecord
            {
                MethodReturn = Expression.Label(typeof(BslValue))
            });
            Symbols.AddScope(new SymbolScope());
            FillParameterVariables();

            try
            {
                visitor(node);
            }
            catch
            {
                _blocks.LeaveBlock();
                throw;
            }
            finally
            {
                Symbols.PopScope();
            }

            var block = _blocks.LeaveBlock();
            var parameters = _localVariables.Take(_declaredParameters.Length).ToArray(); 
            var body = Expression.Block(
                _localVariables.Skip(parameters.Length).ToArray(),
                block.GetStatements());

            var impl = Expression.Lambda(body, parameters);
            
            _method.SetImplementation(impl);
        }

        private void FillParameterVariables()
        {
            _declaredParameters = _method.Parameters.ToArray();
            var localScope = Symbols.TopScope();
            foreach (var parameter in _declaredParameters)
            {
                localScope.AddVariable(parameter.Name, parameter.ParameterType);
                _localVariables.Add(Expression.Parameter(parameter.ParameterType, parameter.Name));
            }
        }

        protected override void VisitMethodVariable(MethodNode method, VariableDefinitionNode variableDefinition)
        {
            var expr = Expression.Variable(typeof(BslValue), variableDefinition.Name);
            _localVariables.Add(expr);
        }

        protected override void VisitStatement(BslSyntaxNode statement)
        {
            _statementBuildParts.Clear();
            try
            {
                base.VisitStatement(statement);
            }
            catch (InternalFlowInterruptException)
            {
                // нижележащий код заполнил коллекцию errors
                // а мы просто переходим к следующей строке кода
            }
        }

        protected override void VisitVariableRead(TerminalNode node)
        {
            if (!Symbols.FindVariable(node.GetIdentifier(), out var binding))
            {
                AddError($"Unknown variable {node.GetIdentifier()}", node.Location);
                return;
            }

            var symbol = Symbols.GetScope(binding.ScopeNumber).Variables[binding.MemberNumber];
            if (symbol.MemberInfo == null)
            {
                // local read
                var expr = _localVariables[binding.MemberNumber];
                _statementBuildParts.Push(expr);
            }
            else
            {
                // prop read
                var target = symbol.Target;
                _statementBuildParts.Push(Expression.Constant(target));
            }
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = Symbols.FindVariable(identifier, out var varBinding);
            if (hasVar)
            {
                var symbol = Symbols.GetScope(varBinding.ScopeNumber).Variables[varBinding.MemberNumber];
                if (symbol.MemberInfo == null)
                {
                    var local = _localVariables[varBinding.MemberNumber];
                    _statementBuildParts.Push(local);
                }
                else
                {
                   var propSymbol = (PropertySymbol) symbol;
                   var convert = Expression.Convert(Expression.Constant(propSymbol.Target),
                            propSymbol.Target.GetType());
                    
                   var accessExpression = Expression.Property(convert, propSymbol.PropertyInfo.SetMethod);
                   _statementBuildParts.Push(accessExpression);
                }
            }
            else
            {
                // can create variable
                var typeOnStack = _statementBuildParts.Peek().Type;

                var scope = Symbols.TopScope();
                scope.AddVariable(identifier, typeOnStack);
                var variable = Expression.Variable(typeOnStack, identifier);
                _localVariables.Add(variable);
                _statementBuildParts.Push(variable);
            }
        }

        protected override void VisitConstant(TerminalNode node)
        {
            object constant = CompilerHelpers.ClrValueFromLiteral(node.Lexem);
            _statementBuildParts.Push(Expression.Constant(constant));
        }
        
        protected override void AddError(BilingualString errorText, CodeRange location)
        {
            base.AddError(errorText, location);
            throw new InternalFlowInterruptException();
        }
    }
}