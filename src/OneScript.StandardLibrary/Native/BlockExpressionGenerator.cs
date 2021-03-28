/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Compiler;
using ScriptEngine.Compiler.Extensions;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;
using Refl = System.Reflection;

namespace OneScript.StandardLibrary.Native
{
    internal class BlockExpressionGenerator : BslSyntaxWalker
    {
        private readonly SymbolResolver _ctx;
        private readonly ITypeManager _typeManager;
        private readonly IErrorSink _errors;
        private ModuleInformation _moduleInfo;
        
        private List<Expression> _statements = new List<Expression>();
        private Stack<Expression> _statementBuildParts = new Stack<Expression>();

        private List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        
        public BlockExpressionGenerator(
            SymbolResolver context,
            ITypeManager typeManager,
            IErrorSink errors)
        {
            _ctx = context;
            _typeManager = typeManager;
            _errors = errors;
        }

        public BlockExpression CreateExpression(CodeBatchNode blockNode, ModuleInformation moduleInfo)
        {
            _moduleInfo = moduleInfo;

            Visit(blockNode);

            return Expression.Block(_localVariables, _statements);
        }

        protected override void VisitStatement(BslSyntaxNode statement)
        {
            _statementBuildParts.Clear();
            base.VisitStatement(statement);
        }

        protected override void VisitGlobalProcedureCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {   
                _errors.AddError(LocalizedErrors.UseBuiltInFunctionAsProcedure());
                return;
            }

            MethodSymbol symbol;
            try
            {
                symbol = _ctx.GetMethod(node.Identifier.GetIdentifier());
                var args = node.ArgumentList.Children.Select(ConvertToExpressionTree);
                
                var expression = CreateClrMethodCall(symbol.Target, symbol.MethodInfo);
                _statements.Add(expression);
            }
            catch (CompilerException e)
            {
                _errors.AddError(e);
            }
        }

        private Expression ConvertToExpressionTree(BslSyntaxNode arg)
        {
            if (arg is TerminalNode term)
            {

                if (arg.Kind == NodeKind.Constant)
                {
                    VisitConstant(term);
                }
                else
                {
                    VisitVariableRead(term);
                }
            }
            else
            {
                DefaultVisit(arg);
            }

            return _statementBuildParts.Pop();
        }

        protected override void VisitVariableRead(TerminalNode node)
        {
            VariableSymbol symbol;
            var identifier = node.GetIdentifier();
            try
            {
                symbol = _ctx.GetVariable(identifier);
                
            }
            catch (CompilerException e)
            {
                _errors.AddError(e); 
                return;
            }
            
            _errors.AddError(new ParseError
            {
                Description = Locale.NStr($"ru='Переменная {identifier} не доступна для чтения';en='Variable {identifier} is not availiable for reading'"),
                Position = node.Lexem.Location.ToCodePosition(_moduleInfo)
            });
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = _ctx.TryGetVariable(identifier, out var varBinding);
            if (hasVar)
            {
                if (varBinding.PropertyInfo == null)
                {
                    //AddCommand(OperationCode.LoadLoc, varBinding.binding.CodeIndex);
                }
                else
                {
                    _errors.AddError(new ParseError
                    {
                        Description = Locale.NStr($"ru='Переменная {identifier} не доступна для записи';en='Variable {identifier} is not availiable for write'"),
                        Position = node.Lexem.Location.ToCodePosition(_moduleInfo)
                    });
                }
            }
            else
            {
                // can create variable
                var typeOnStack = _statementBuildParts.Peek().Type;
                _ctx.AddVariable(identifier, typeOnStack);
                var variable = Expression.Variable(typeOnStack, identifier);
                _localVariables.Add(variable, identifier);
                _statementBuildParts.Push(variable);
                var assign = Assign();
                _statements.Add(assign);
            }
        }

        protected override void VisitConstant(TerminalNode node)
        {
            DataType constType = default;
            Type clrType = default;
            
            switch (node.Lexem.Type)
            {
                case LexemType.BooleanLiteral:
                    constType = DataType.Boolean;
                    clrType = typeof(bool);
                    break;
                case LexemType.DateLiteral:
                    constType = DataType.Date;
                    clrType = typeof(DateTime);
                    break;
                case LexemType.NumberLiteral:
                    constType = DataType.Number;
                    clrType = typeof(decimal);
                    break;
                case LexemType.StringLiteral:
                    constType = DataType.String;
                    clrType = typeof(string);
                    break;
                case LexemType.NullLiteral:
                    constType = DataType.GenericValue;
                    clrType = typeof(NullValue);
                    break;
                case LexemType.UndefinedLiteral:
                    constType = DataType.Undefined;
                    clrType = typeof(object);
                    break;
            }

            var data = ValueFactory.Parse(node.Lexem.Content, constType);
            Expression expr;

            switch (data.DataType)
            {
                case DataType.Undefined:
                case DataType.GenericValue:
                    expr = Expression.Constant(null, typeof(object));
                    break;
                case DataType.String:
                    expr = Expression.Constant(data.AsString());
                    break;
                case DataType.Number:
                    expr = Expression.Constant(data.AsNumber());
                    break;
                case DataType.Date:
                    expr = Expression.Constant(data.AsDate());
                    break;
                case DataType.Boolean:
                    expr = Expression.Constant(data.AsBoolean());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _statementBuildParts.Push(expr);
        }

        private Expression Assign()
        {
            var left = _statementBuildParts.Pop();
            var right = _statementBuildParts.Pop();
            var assign = Expression.Assign(left, right);

            return assign;
        }

        private Expression CreateClrMethodCall(IRuntimeContextInstance context, Refl.MethodInfo method)
        {
            // этот контекст или класс контекстный или скриптовый метод
            // для скриптовых методов мы сделаем обычный вызовем через машину
            // для контекстов - сделаем прямой вызов метода [ContextMethod] с типизацией параметров
            throw new NotImplementedException();
        }
    }
}