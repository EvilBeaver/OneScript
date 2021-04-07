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
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    public class MethodCompiler : ExpressionTreeGeneratorBase
    {
        private readonly BslMethodInfo _method;
        private readonly List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        private readonly StatementBlocksWriter _blocks = new StatementBlocksWriter();
        private readonly Stack<Expression> _statementBuildParts = new Stack<Expression>();
        
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
            CompileFragment(moduleBodyNode.Children[0], VisitCodeBlock);
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
            var parameters = _localVariables.Take(_method.GetParameters().Length).ToArray(); 
            var body = Expression.Block(
                _localVariables.Skip(parameters.Length).ToArray(),
                block.GetStatements());

            var impl = Expression.Lambda(body, parameters);
            
            _method.SetImplementation(impl);
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

        protected override void AddError(BilingualString errorText, CodeRange location)
        {
            base.AddError(errorText, location);
            throw new InternalFlowInterruptException();
        }
    }
}