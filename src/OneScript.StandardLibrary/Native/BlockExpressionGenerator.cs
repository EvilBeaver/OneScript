/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;
using Refl = System.Reflection;

namespace OneScript.StandardLibrary.Native
{
    internal class BlockExpressionGenerator : BslSyntaxWalker
    {
        private readonly SymbolTable _ctx;
        private readonly ITypeManager _typeManager;
        private readonly IErrorSink _errors;
        private ModuleInformation _moduleInfo;
        
        private List<Expression> _statements = new List<Expression>();
        private Stack<Expression> _statementBuildParts = new Stack<Expression>();

        private List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        private LabelTarget _fragmentReturn;
        
        private int _parametersCount = 0;

        private class InternalFlowInterruptException : Exception
        {
        }
        
        public BlockExpressionGenerator(
            SymbolTable context,
            ITypeManager typeManager,
            IErrorSink errors)
        {
            _ctx = context;
            _typeManager = typeManager;
            _errors = errors;

            if (context.TopScope() != null)
            {
                foreach (var local in context.TopScope().Variables)
                {
                    _localVariables.Add(Expression.Variable(local.VariableType, local.Name));
                    ++_parametersCount;
                }
            }

            // куда будут прыгать все Возвраты из метода
            _fragmentReturn = Expression.Label(typeof(IValue));
        }

        public LambdaExpression CreateExpression(ModuleNode module, ModuleInformation moduleInfo)
        {
            _moduleInfo = moduleInfo;

            Visit(module);

            AppendReturnValue();
            
            var body = Expression.Block(_localVariables, _statements);
            var parameters = _localVariables.Take(_parametersCount);

            return Expression.Lambda(body, parameters);
        }

        private void AppendReturnValue()
        {
            var convertMethod = typeof(ContextValuesMarshaller)
                .GetMethod(nameof(ContextValuesMarshaller.ConvertReturnValue),
                    new Type[] {typeof(object), typeof(Type)});
            
            Debug.Assert(convertMethod != null);

            // возврат Неопределено по умолчанию
            var jumpLabel = Expression.Label(_fragmentReturn, Expression.Constant(ValueFactory.Create()));
            _statements.Add(jumpLabel);
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

        protected override void VisitGlobalProcedureCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {   
                _errors.AddError(LocalizedErrors.UseBuiltInFunctionAsProcedure());
                return;
            }

            if (!_ctx.FindMethod(node.Identifier.GetIdentifier(), out var binding))
            {
                AddError($"Unknown method {node.Identifier.GetIdentifier()}", node.Location);
                return;
            }

            var symbol = _ctx.GetScope(binding.ContextIndex).Methods[binding.CodeIndex];
            var args = node.ArgumentList.Children.Select(ConvertToExpressionTree);
            
            var expression = CreateClrMethodCall(symbol.Target, symbol.MethodInfo, args);
            _statements.Add(expression);
        }

        private Expression ConvertToExpressionTree(BslSyntaxNode arg)
        {
            VisitExpression(arg);
            return _statementBuildParts.Pop();
        }

        protected override void VisitAssignment(BslSyntaxNode assignment)
        {
            var left = assignment.Children[0];
            var right = assignment.Children[1];
            
            VisitExpression(right);
            if (left is TerminalNode t)
            {
                VisitVariableWrite(t);
            }
            else
            {
                VisitReferenceRead(left);
            }

            var expr = MakeAssign();
            _statements.Add(expr);
        }

        protected override void VisitVariableRead(TerminalNode node)
        {
            if (!_ctx.FindVariable(node.GetIdentifier(), out var binding))
            {
                AddError($"Unknown variable {node.GetIdentifier()}", node.Location);
                return;
            }

            var symbol = _ctx.GetScope(binding.ContextIndex).Variables[binding.CodeIndex];
            if (symbol.MemberInfo == null)
            {
                // local read
                var expr = _localVariables[binding.CodeIndex];
                _statementBuildParts.Push(expr);
            }
            else
            {
                // prop read
                throw new NotImplementedException("yet");
            }
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = _ctx.FindVariable(identifier, out var varBinding);
            if (hasVar)
            {
                var symbol = _ctx.GetScope(varBinding.ContextIndex).Variables[varBinding.CodeIndex];
                if (symbol.MemberInfo == null)
                {
                    var local = _localVariables[varBinding.CodeIndex];
                    _statementBuildParts.Push(local);
                }
                else
                {
                    var propSymbol = (PropertySymbol) symbol;

                    if (propSymbol.Target is DynamicPropertiesAccessor)
                    {
                        var call = ExpressionHelpers.CallToSetPropertyProtocol(
                            propSymbol.Target,
                            identifier,
                            Expression.Parameter(typeof(IValue)));
                        
                        _statementBuildParts.Push(call);
                    }
                    else
                    {
                        var convert = Expression.Convert(Expression.Constant(propSymbol.Target),
                            propSymbol.Target.GetType());
                    
                        var accessExpression = Expression.Property(convert, propSymbol.PropertyInfo.SetMethod);
                        _statementBuildParts.Push(accessExpression);
                    }
                }
            }
            else
            {
                // can create variable
                var typeOnStack = _statementBuildParts.Peek().Type;

                var scope = _ctx.TopScope();
                SymbolScope.AddVariable(scope, identifier, typeOnStack);
                var variable = Expression.Variable(typeOnStack, identifier);
                _localVariables.Add(variable);
                _statementBuildParts.Push(variable);
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

        protected override void VisitBinaryOperation(BinaryOperationNode binaryOperationNode)
        {
            VisitExpression(binaryOperationNode.Children[0]);
            VisitExpression(binaryOperationNode.Children[1]);

            var right = _statementBuildParts.Pop();
            var left = _statementBuildParts.Pop();
            
            var binaryOp = DispatchBinaryOp(left, right, binaryOperationNode);
            
            if (LanguageDef.IsLogicalBinaryOperator(binaryOperationNode.Operation))
            {
                var toBool = Expression.Convert(binaryOp, typeof(bool));
                _statementBuildParts.Push(toBool);
            }
            else
            {
                _statementBuildParts.Push(binaryOp);
            }
        }

        private Expression DispatchBinaryOp(Expression left, Expression right, BinaryOperationNode binaryOperationNode)
        {
            var compiler = new BinaryOperationCompiler();
            try
            {
                return compiler.Compile(binaryOperationNode, left, right);
            }
            catch (CompilerException e)
            {
                AddError(e.Message, binaryOperationNode.Location);
                return null;
            }
        }

        protected override void VisitUnaryOperation(UnaryOperationNode unaryOperationNode)
        {
            var child = unaryOperationNode.Children[0];
            VisitExpression(child);
            var opCode = TokenToOperationCode(unaryOperationNode.Operation);

            Type resultType = null;
            switch (opCode)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    resultType = typeof(decimal);
                    break;
                case ExpressionType.Not:
                    resultType = typeof(bool);
                    break;
            }
            
            var operation = Expression.MakeUnary(opCode, _statementBuildParts.Pop(), resultType);
            _statementBuildParts.Push(operation);
        }

        private static ExpressionType TokenToOperationCode(Token stackOp) =>
            ExpressionHelpers.TokenToOperationCode(stackOp);

        private Expression MakeAssign()
        {
            var left = _statementBuildParts.Pop();
            var right = _statementBuildParts.Pop();

            if (left is MethodCallExpression call)
            {
                if (typeof(IValue).IsAssignableFrom(right.Type))
                {
                    return Expression.Invoke(call, right);
                }
                else
                {
                    return Expression.Invoke(call, ExpressionHelpers.ConvertToIValue(right));
                }
            }
            else
            {
                return Expression.Assign(left, right);
            }
        }

        private Expression CreateClrMethodCall(IRuntimeContextInstance context, Refl.MethodInfo method,
            IEnumerable<Expression> expressions)
        {
            // этот контекст или класс контекстный или скриптовый метод
            // для скриптовых методов мы сделаем обычный вызовем через машину
            // для контекстов - сделаем прямой вызов метода [ContextMethod] с типизацией параметров
            throw new NotImplementedException();
        }

        private void AddError(string text, CodeRange position)
        {
            var err = new ParseError
            {
                Description = text,
                Position = position.ToCodePosition(_moduleInfo),
                ErrorId = "TreeCompilerErr"
            };
                
            _errors.AddError(err);
            
            throw new InternalFlowInterruptException();
        }
    }
}