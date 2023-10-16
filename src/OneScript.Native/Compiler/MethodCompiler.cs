/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using OneScript.Commons;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Exceptions;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Localization;
using OneScript.Native.Runtime;
using OneScript.Sources;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Native.Compiler
{
    public class MethodCompiler : ExpressionTreeGeneratorBase
    {
        private readonly BslNativeMethodInfo _method;
        private readonly List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        private readonly StatementBlocksWriter _blocks = new StatementBlocksWriter();
        private readonly Stack<Expression> _statementBuildParts = new Stack<Expression>();
        private BslParameterInfo[] _declaredParameters;
        private ParameterExpression _thisParameter;
        
        private readonly BinaryOperationCompiler _binaryOperationCompiler = new BinaryOperationCompiler();
        private ITypeManager _typeManager;
        private IExceptionInfoFactory _exceptionInfoFactory;
        private readonly IServiceContainer _services;
        private readonly BuiltInFunctionsCache _builtInFunctions = new BuiltInFunctionsCache();

        private readonly ContextMethodsCache _methodsCache = new ContextMethodsCache();
        private readonly ContextPropertiesCache _propertiesCache = new ContextPropertiesCache();
        private readonly SourceCode _source;

        public MethodCompiler(BslWalkerContext walkContext, BslNativeMethodInfo method) : base(walkContext)
        {
            _method = method;
            _services = walkContext.Services;
            _source = walkContext.Source;
        }

        private ITypeManager CurrentTypeManager => GetService(_services, ref _typeManager);

        private IExceptionInfoFactory ExceptionInfoFactory => GetService(_services, ref _exceptionInfoFactory);

        private static T GetService<T>(IServiceContainer services, ref T value) where T : class
        {
            if (value != default) 
                return value;
                
            value = services.TryResolve<T>();
            if (value == default)
                throw new NotSupportedException($"{typeof(T)} is not registered in services.");

            return value;
        }
        
        public void CompileMethod(MethodNode methodNode)
        {
            CompileFragment(methodNode, x=>VisitMethodBody((MethodNode)x));
        }
        
        public void CompileModuleBody(BslSyntaxNode moduleBodyNode)
        {
            if(moduleBodyNode != default)
                CompileFragment(moduleBodyNode, VisitModuleBody);
            else
                CompileFragment(null, n => {});
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
            Symbols.PushScope(new SymbolScope(), null);
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
            block.Add(Expression.Label(
                block.MethodReturn, 
                Expression.Constant(BslUndefinedValue.Instance)));

            var parameters = new List<ParameterExpression>();
            if (_method.IsInstance)
                parameters.Add(_thisParameter);
            
            parameters.AddRange(_localVariables.Take(_declaredParameters.Length));
            var blockVariables = _localVariables.Skip(_declaredParameters.Length);
            
            var body = Expression.Block(
                blockVariables.ToArray(),
                block.GetStatements());

            var impl = Expression.Lambda(body, parameters);
            
            _method.SetImplementation(impl);
        }

        private void FillParameterVariables()
        {
            _declaredParameters = _method.GetBslParameters();
            _thisParameter = _method.IsInstance ? Expression.Parameter(typeof(NativeClassInstanceWrapper), "$this") : null;

            var localScope = Symbols.GetScope(Symbols.ScopeCount-1);
            foreach (var parameter in _declaredParameters)
            {
                var paramSymbol = new LocalVariableSymbol(parameter.Name, parameter.ParameterType);
                localScope.DefineVariable(paramSymbol);
                _localVariables.Add(Expression.Parameter(parameter.ParameterType, parameter.Name));
            }
        }

        private Expression DefineLocalVariable(string identifier, Type type)
        {
            var varSymbol = new LocalVariableSymbol(identifier, type);
            var scope = Symbols.GetScope(Symbols.ScopeCount - 1);
            scope.DefineVariable(varSymbol);
            var variable = Expression.Variable(type, identifier);
            _localVariables.Add(variable);
            return variable;
        }

        protected override void VisitMethodVariable(MethodNode method, VariableDefinitionNode variableDefinition)
        {
            var identifier = variableDefinition.Name;
            if (Symbols.FindVariable(identifier, out var binding) && binding.ScopeNumber == Symbols.ScopeCount - 1)
            {
                AddError(LocalizedErrors.DuplicateVarDefinition(identifier));
                return;
            }
            
            var variable = DefineLocalVariable(identifier, typeof(BslValue));
            _blocks.Add(Expression.Assign(variable, Expression.Constant(BslUndefinedValue.Instance)));
         }

        protected override void VisitStatement(BslSyntaxNode statement)
        {
            _statementBuildParts.Clear();
            var nestingLevel = _blocks.Count;
            try
            {
                base.VisitStatement(statement);
            }
            catch (InternalFlowInterruptException)
            {
                // нижележащий код заполнил коллекцию errors
                // а мы просто переходим к следующей строке кода
                RestoreNestingLevel(nestingLevel);
            }
            catch (NativeCompilerException e)
            {
                RestoreNestingLevel(nestingLevel);
                e.SetPositionIfEmpty(ToCodePosition(statement.Location));
                AddError(new CodeError
                {
                    Description = e.Message,
                    Position = e.GetPosition(),
                    ErrorId = nameof(NativeCompilerException)
                });
            }
            catch (Exception e)
            {
                
                RestoreNestingLevel(nestingLevel);
                if (e is NativeCompilerException ce)
                {
                    ce.SetPositionIfEmpty(ToCodePosition(statement.Location));
                    throw;
                }
                
                var msg = new BilingualString(
                    "Ошибка компиляции статического модуля\n" + e,
                    "Error compiling static module\n" + e);
                base.AddError(msg, statement.Location);
            }
        }

        private void RestoreNestingLevel(int desiredLevel)
        {
            while (_blocks.Count > desiredLevel)
            {
                _blocks.LeaveBlock();
            }
        }
        
        #region Expressions
        
        protected override void VisitVariableRead(TerminalNode node)
        {
            if (!Symbols.FindVariable(node.GetIdentifier(), out var binding))
            {
                AddError(LocalizedErrors.SymbolNotFound(node.GetIdentifier()), node.Location);
                return;
            }

            var symbol = Symbols.GetScope(binding.ScopeNumber).Variables[binding.MemberNumber];
            if (IsLocalScope(binding.ScopeNumber))
            {
                // local read
                var expr = GetLocalVariable(binding.MemberNumber);
                _statementBuildParts.Push(expr);
            }
            else if (symbol is IPropertySymbol prop)
            {
                var expression = ReadGlobalProperty(Symbols.GetBinding(binding.ScopeNumber), prop);
                _statementBuildParts.Push(expression);
            }
            else if (symbol is IFieldSymbol && _method.IsInstance)
            {
                var iVariableVal = ExpressionHelpers.AccessModuleVariable(_thisParameter, binding.MemberNumber);
                _statementBuildParts.Push(Expression.Convert(iVariableVal, typeof(BslValue)));
            }
            else
            {
                AddError(new BilingualString($"Unsupported symbol in non-local context: {symbol.Name} {symbol.GetType()}"), 
                    node.Location);
            }
        }

        private Expression ReadGlobalProperty(IRuntimeContextInstance target, IPropertySymbol prop)
        {
            var propInfo = prop.Property;

            if (propInfo is ContextPropertyInfo contextProp)
            {
                return Expression.MakeMemberAccess(
                    Expression.Constant(target),
                    contextProp.GetUnderlying<PropertyInfo>());
            }
            else
            {
                var propIdx = target.GetPropertyNumber(prop.Name);
                return ExpressionHelpers.GetContextPropertyValue(target, propIdx);
            }
        }

        private object GetPropertyBinding(SymbolBinding binding, IVariableSymbol symbol)
        {
            return Symbols.GetBinding(binding.ScopeNumber) ?? 
                   throw new InvalidOperationException($"Property {symbol.Name} is not bound to a value");
        }
        
        private object GetMethodBinding(SymbolBinding binding, IMethodSymbol symbol)
        {
            return Symbols.GetBinding(binding.ScopeNumber) ?? 
                   throw new InvalidOperationException($"Method {symbol.Name} is not bound to a value");
        }

        private void MakeReadPropertyAccess(TerminalNode operand)
        {
            var memberName = operand.Lexem.Content;
            var instance = _statementBuildParts.Pop();
            var instanceType = instance.Type;
            var prop = TryFindPropertyOfType(operand, instanceType, memberName);

            if (prop != null)
            {
                var expression = Expression.MakeMemberAccess(instance, prop);

                if (typeof(IValue) == expression.Type)
                {
                    _statementBuildParts.Push(Expression.TypeAs(expression, typeof(BslValue)));
                    return;
                }

                _statementBuildParts.Push(expression);
                return;
            }

            var expr = ExpressionHelpers.GetContextPropertyValue(instance, memberName);
            _statementBuildParts.Push(expr);
        }

        private void MakeWritePropertyAccess(TerminalNode operand)
        {
            var memberName = operand.Lexem.Content;
            var instance = _statementBuildParts.Pop();
            var instanceType = instance.Type;
            var prop = TryFindPropertyOfType(operand, instanceType, memberName);

            if (prop != null)
            {
                _statementBuildParts.Push(Expression.MakeMemberAccess(instance, prop));
                return;
            }

            var valueToSet = _statementBuildParts.Pop();

            var args = new List<Expression>();
            args.Add(instance);
            args.Add(valueToSet);

            var csharpArgs = new List<CSharpArgumentInfo>();
            csharpArgs.Add(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default));
            csharpArgs.AddRange(args.Select(x =>
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default)));

            var binder = Microsoft.CSharp.RuntimeBinder.Binder.SetMember(
                CSharpBinderFlags.InvokeSimpleName,
                memberName,
                typeof(BslObjectValue), csharpArgs);
            var expr = Expression.Dynamic(binder, typeof(object), args);
            _statementBuildParts.Push(expr);
        }

        protected override void VisitResolveProperty(TerminalNode operand)
        {
            MakeReadPropertyAccess(operand);
        }

        private ParameterExpression GetLocalVariable(int index) => _localVariables[index];

        private bool IsLocalScope(int scopeNumber) => scopeNumber == Symbols.ScopeCount - 1;
        
        private bool IsModuleScope(int scopeNumber) => scopeNumber == Symbols.ScopeCount - 2;
        
        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = Symbols.FindVariable(identifier, out var varBinding);
            if (hasVar)
            {
                var symbol = Symbols.GetScope(varBinding.ScopeNumber).Variables[varBinding.MemberNumber];
                if (IsLocalScope(varBinding.ScopeNumber))
                {
                    var local = GetLocalVariable(varBinding.MemberNumber);
                    _statementBuildParts.Push(local);
                }
                else if (symbol is IPropertySymbol propSymbol)
                {
                    var target = GetPropertyBinding(varBinding, propSymbol);
                    var convert = Expression.Convert(Expression.Constant(target),
                            target.GetType());
                    
                   var accessExpression = Expression.Property(convert, propSymbol.Property.SetMethod);
                   _statementBuildParts.Push(accessExpression);
                }
                else if (symbol is IFieldSymbol && _method.IsInstance)
                {
                    var iVariableVal = ExpressionHelpers.AccessModuleVariable(_thisParameter, varBinding.MemberNumber);
                    _statementBuildParts.Push(iVariableVal);
                }
                else
                {
                    AddError(new BilingualString($"Unsupported symbol in non-local context: {symbol.Name} {symbol.GetType()}"), 
                        node.Location);
                }
            }
            else
            {
                // can create variable
                var typeOnStack = _statementBuildParts.Peek().Type;

                if (typeOnStack == typeof(BslUndefinedValue))
                    typeOnStack = typeof(BslValue);
                if (typeOnStack.IsNumeric())
                    typeOnStack = typeof(decimal);

                _statementBuildParts.Push( DefineLocalVariable(identifier, typeOnStack) );
            }
        }

        protected override void VisitConstant(TerminalNode node)
        {
            object constant = CompilerHelpers.ClrValueFromLiteral(node.Lexem);
            _statementBuildParts.Push(Expression.Constant(constant));
        }
        
        protected override void VisitAssignment(BslSyntaxNode assignment)
        {
            var astLeft = assignment.Children[0];
            var astRight = assignment.Children[1];
            
            VisitAssignmentRightPart(astRight);
            VisitAssignmentLeftPart(astLeft);
            
            var left = _statementBuildParts.Pop();
            
            if (left.NodeType == ExpressionType.Dynamic && left is DynamicExpression dyn)
            {
                if (dyn.Binder is SetIndexBinder || dyn.Binder is SetMemberBinder)
                {
                    _blocks.Add(left);
                    return;
                }

                throw new NotSupportedException($"Dynamic operation {dyn.Binder} is not supported");
            }
            else if (left.NodeType == ExpressionType.Call)
            {
                _blocks.Add(left);
                return;
            }
            
            var right = ExpressionHelpers.CreateAssignmentSource(_statementBuildParts.Pop(), left.Type);
            
            _blocks.Add(Expression.Assign(left, right));
        }

        protected override void VisitAssignmentLeftPart(BslSyntaxNode node)
        {
            if (node is TerminalNode t)
            {
                VisitVariableWrite(t);
            }
            else if (node.Kind == NodeKind.IndexAccess)
            {
                 VisitIndexAccessWrite(node);
            }
            else
            {
                VisitReferenceWrite(node);
            }
        }

        protected override void VisitReferenceWrite(BslSyntaxNode node)
        {
            var instanceNode = node.Children[0];
            var memberNode = node.Children[1] as TerminalNode;

            if (instanceNode is TerminalNode terminalNode)
            {
                VisitVariableRead(terminalNode);
            }
            else
            {
                DefaultVisit(instanceNode);
            }

            MakeWritePropertyAccess(memberNode);
        }

        protected override void VisitBinaryOperation(BinaryOperationNode binaryOperationNode)
        {
            VisitExpression(binaryOperationNode.Children[0]);
            VisitExpression(binaryOperationNode.Children[1]);

            var right = _statementBuildParts.Pop();
            var left = _statementBuildParts.Pop();
            
            var binaryOp = CompileBinaryOp(left, right, binaryOperationNode);
            
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

        protected override void VisitUnaryOperation(UnaryOperationNode unaryOperationNode)
        {
            var child = unaryOperationNode.Children[0];
            VisitExpression(child);
            var opCode = ExpressionHelpers.TokenToOperationCode(unaryOperationNode.Operation);

            Type resultType = null;
            Expression argument = _statementBuildParts.Pop();
            switch (opCode)
            {
                case ExpressionType.UnaryPlus:
                case ExpressionType.Negate:
                    resultType = typeof(decimal);
                    argument = ExpressionHelpers.ToNumber(argument);
                    break;
                case ExpressionType.Not:
                    argument = ExpressionHelpers.ToBoolean(argument);
                    resultType = typeof(bool);
                    break;
            }

            var operation = Expression.MakeUnary(opCode, argument, resultType);
            _statementBuildParts.Push(operation);
        }

        protected override void VisitTernaryOperation(BslSyntaxNode node)
        {
            Debug.Assert(node.Children.Count == 3);

            var test = ConvertToExpressionTree(node.Children[0]);
            var ifTrue = ConvertToExpressionTree(node.Children[1]);
            var ifFalse = ConvertToExpressionTree(node.Children[2]);

            if (ifTrue.Type != ifFalse.Type)
            {
                (ifTrue, ifFalse) = ExpressionHelpers.ConvertToCompatibleBslValues(ifTrue, ifFalse);
            }

            _statementBuildParts.Push(Expression.Condition(ExpressionHelpers.ToBoolean(test), ifTrue, ifFalse));
        }

        #region Dereferencing

        protected override void VisitIndexAccess(BslSyntaxNode node)
        {
            base.VisitIndexAccess(node);
            
            var index = _statementBuildParts.Pop();
            var target = _statementBuildParts.Pop();

            var indexExpression = TryCreateIndexExpression(node, target, index);

            if (indexExpression == null)
            {
                indexExpression = TryFindMagicGetterMethod(target, index) 
                                  ?? ExpressionHelpers.GetIndexedValue(target, index);
            }
            
            _statementBuildParts.Push(indexExpression);
        }

        private void VisitIndexAccessWrite(BslSyntaxNode node)
        {
            base.VisitIndexAccess(node);
            
            var index = _statementBuildParts.Pop();
            var target = _statementBuildParts.Pop();
            
            var indexExpression = TryCreateIndexExpression(node, target, index);

            if (indexExpression == null)
            {
                var value = _statementBuildParts.Pop();
                indexExpression = TryFindMagicSetterMethod(target, index, value) 
                                  ?? ExpressionHelpers.SetIndexedValue(target, index, value);
            }
            
            _statementBuildParts.Push(indexExpression);
        }

        private Expression TryFindMagicGetterMethod(Expression target, Expression index)
        {
            var targetType = target.Type;
            var method = targetType.GetMethod("BslIndexGetter");
            if (method == null)
                return null;

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return null;

            var indexParamType = parameters[0].ParameterType;
            
            return Expression.Call(target, method,ExpressionHelpers.ConvertToType(index, indexParamType));
        }
        
        private Expression TryFindMagicSetterMethod(Expression target, Expression index, Expression value)
        {
            var targetType = target.Type;
            var method = targetType.GetMethod("BslIndexSetter");
            if (method == null)
                return null;

            var parameters = method.GetParameters();
            if (parameters.Length != 2)
                return null;

            var indexParamType = parameters[0].ParameterType;
            var valueType = parameters[1].ParameterType;

            return Expression.Call(target, method,
                ExpressionHelpers.ConvertToType(index, indexParamType),
                ExpressionHelpers.ConvertToType(value, valueType));

        }

        private Expression TryCreateIndexExpression(BslSyntaxNode node, Expression target, Expression index)
        {
            if (!typeof(BslObjectValue).IsAssignableFrom(target.Type))
            {
                // Мы не знаем тип выражения и будем выяснять это в рантайме
                return null;
            }
            
            var indexerProps = target.Type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetIndexParameters().Length != 0)
                .ToList();

            Expression indexExpression = null;
            
            if(indexerProps.Count > 1)
            {
                foreach (var propertyInfo in indexerProps)
                {
                    var parameterType = propertyInfo.GetIndexParameters()[0].ParameterType;
                    var passExpression = ExpressionHelpers.TryConvertParameter(index, parameterType);
                    
                    if (passExpression == null) 
                        continue;
                    
                    indexExpression = Expression.MakeIndex(target, propertyInfo, new[] {passExpression});
                    break;
                }
            }
            else if(indexerProps.Count == 1)
            {
                var propInfo = indexerProps[0];
                var parameterType = propInfo.GetIndexParameters()[0].ParameterType;
                var passExpression = ExpressionHelpers.TryConvertParameter(index, parameterType);
                if(passExpression != null)
                    indexExpression = Expression.MakeIndex(target, propInfo, new[]{passExpression});
            }

            return indexExpression;
        }
        
        #endregion
        
        private Expression CompileBinaryOp(Expression left, Expression right, BinaryOperationNode binaryOperationNode)
        {
            try
            {
                return _binaryOperationCompiler.Compile(binaryOperationNode, left, right);
            }
            catch (NativeCompilerException e)
            {
                AddError(e.Message, binaryOperationNode.Location);
                return null;
            }
        }
        
        protected override void VisitReturnNode(BslSyntaxNode node)
        {
            if (_method.IsFunction() && node.Children.Count == 0)
            {
                AddError(LocalizedErrors.FuncEmptyReturnValue(), node.Location);
                return;
            }
            
            var label = _blocks.GetCurrentBlock().MethodReturn;

            if (!_method.IsFunction() && node.Children.Count == 0)
            {
                var undefinedExpr = Expression.Constant(BslUndefinedValue.Instance);
                _blocks.Add(Expression.Return(label, undefinedExpr));
                return;
            }

            VisitExpression(node.Children[0]);

            var resultExpr = _statementBuildParts.Pop();
            
            if (!resultExpr.Type.IsValue())
                resultExpr = ExpressionHelpers.ConvertToType(resultExpr, typeof(BslValue));
            
            var statement = Expression.Return(label, resultExpr);
            _blocks.Add(statement);
        }
        
        #endregion
        
        #region If Block
        
        protected override void VisitIfNode(ConditionNode node)
        {
            _blocks.EnterBlock();
            
            VisitIfExpression(node.Expression);
            VisitIfTruePart(node.TruePart);

            var stack = new Stack<ConditionNode>();
            foreach (var alternative in node.GetAlternatives())
            {
                if (alternative is ConditionNode elif)
                {
                    stack.Push(elif);
                }
                else if (stack.Count > 0)
                {
                    var cond = stack.Pop();

                    VisitElseNode((CodeBatchNode) alternative);
                    VisitElseIfNode(cond);
                }
                else
                {
                    VisitElseNode((CodeBatchNode) alternative);
                }
            }

            while (stack.Count > 0)
            {
                var elseIfNode = stack.Pop();
                VisitElseIfNode(elseIfNode);
            }
  

            var expression = CreateIfExpression(_blocks.LeaveBlock());
            _blocks.Add(expression);
        }

        private Expression CreateIfExpression(StatementsBlockRecord block)
        {
            if (block.BuildStack.Count == 3)
            {
                var falsePart = block.BuildStack.Pop();
                var truePart = block.BuildStack.Pop();
                var condition = block.BuildStack.Pop();

                return Expression.IfThenElse(ExpressionHelpers.ToBoolean(condition), truePart, falsePart);
            }
            else
            {
                Debug.Assert(block.BuildStack.Count == 2);
                var truePart = block.BuildStack.Pop();
                var condition = block.BuildStack.Pop();
                if (condition.Type != typeof(bool))
                    condition = Expression.Convert(condition, typeof(bool));
                
                return Expression.IfThen(ExpressionHelpers.ToBoolean(condition), truePart);
            }
        }

        protected override void VisitIfExpression(BslSyntaxNode node)
        {
            var condition = ConvertToExpressionTree(node);
            _blocks.GetCurrentBlock().BuildStack.Push(condition);
        }

        protected override void VisitIfTruePart(CodeBatchNode node)
        {
            _blocks.EnterBlock();
            base.VisitIfTruePart(node);
            
            var body = _blocks.LeaveBlock().GetStatements();
            _blocks.GetCurrentBlock().BuildStack.Push(Expression.Block(body));
        }

        protected override void VisitElseIfNode(ConditionNode node)
        {
            var hasCompiledElse = _blocks.GetCurrentBlock().BuildStack.Count == 3;
            Expression elseNode = null;
            if (hasCompiledElse)
                elseNode = _blocks.GetCurrentBlock().BuildStack.Pop();
            
            _blocks.EnterBlock();
            VisitIfExpression(node.Expression);
            VisitIfTruePart(node.TruePart);
            
            if(hasCompiledElse)
                _blocks.GetCurrentBlock().BuildStack.Push(elseNode);
            
            var expr = CreateIfExpression(_blocks.LeaveBlock());
            _blocks.GetCurrentBlock().BuildStack.Push(expr);
        }

        protected override void VisitElseNode(CodeBatchNode node)
        {
            _blocks.EnterBlock();
            try
            {
                base.VisitElseNode(node);
                var block = _blocks.LeaveBlock();
                var body = Expression.Block(block.GetStatements());
                _blocks.GetCurrentBlock().BuildStack.Push(body);
            }
            catch
            {
                _blocks.LeaveBlock();
                throw;
            }
        }

        #endregion
        
        #region While Loop

        protected override void VisitWhileNode(WhileLoopNode node)
        {
            _blocks.EnterBlock(new JumpInformationRecord()
            {
                LoopBreak = Expression.Label(),
                LoopContinue = Expression.Label()
            });
            base.VisitWhileNode(node);

            var block = _blocks.LeaveBlock();

            var result = new List<Expression>();
            result.Add(Expression.IfThen(
                Expression.Not(block.BuildStack.Pop()), 
                Expression.Break(block.LoopBreak)));
            
            result.AddRange(block.GetStatements());
            
            var loop = Expression.Loop(Expression.Block(result), block.LoopBreak, block.LoopContinue);
            _blocks.Add(loop);
        }

        protected override void VisitWhileCondition(BslSyntaxNode node)
        {
            var expr = ExpressionHelpers.ToBoolean(ConvertToExpressionTree(node));  
            _blocks.GetCurrentBlock().BuildStack.Push(expr);
        }
        
        protected override void VisitContinueNode(LineMarkerNode node)
        {
            var label = _blocks.GetCurrentBlock().LoopContinue;
            _blocks.Add(Expression.Continue(label));
        }

        protected override void VisitBreakNode(LineMarkerNode node)
        {
            var label = _blocks.GetCurrentBlock().LoopBreak;
            _blocks.Add(Expression.Break(label));
        }

        #endregion
        
        #region For With Counter Loop

        protected override void VisitForLoopNode(ForLoopNode node)
        {
            _blocks.EnterBlock(new JumpInformationRecord
            {
                LoopBreak = Expression.Label(),
                LoopContinue = Expression.Label()
            });
            base.VisitForLoopNode(node);
            var block = _blocks.LeaveBlock();

            var upperLimit = block.BuildStack.Pop();
            var initialValue = block.BuildStack.Pop();
            var counterVar = block.BuildStack.Pop();
            
            var result = new List<Expression>();
            result.Add(Expression.Assign(counterVar, ExpressionHelpers.CreateAssignmentSource(initialValue, counterVar.Type)));
            var finalVar = Expression.Variable(typeof(decimal)); // TODO: BslNumericValue ?
            result.Add(Expression.Assign(finalVar, upperLimit));
            
            var loop = new List<Expression>();
            loop.Add(Expression.IfThen(
                Expression.GreaterThan(ExpressionHelpers.ToNumber(counterVar), finalVar), 
                Expression.Break(block.LoopBreak)));
            
            loop.AddRange(block.GetStatements());
            
            loop.Add(Expression.Label(block.LoopContinue));
            loop.Add(ExpressionHelpers.Increment(counterVar));

            result.Add(Expression.Loop(Expression.Block(loop), block.LoopBreak));
            
            _blocks.Add(Expression.Block(new[] {finalVar}, result));
        }

        protected override void VisitForInitializer(BslSyntaxNode node)
        {
            var forLoopIterator = node.Children[0];
            var forLoopInitialValue = node.Children[1];
            VisitForLoopInitialValue(forLoopInitialValue);
            VisitForLoopIterator(forLoopIterator);
            
            // counter variable
            _blocks.GetCurrentBlock().BuildStack.Push(_statementBuildParts.Pop());
            // initial value
            _blocks.GetCurrentBlock().BuildStack.Push(_statementBuildParts.Pop());
        }

        protected override void VisitForLoopInitialValue(BslSyntaxNode node)
        {
            base.VisitForLoopInitialValue(node);
            var expr = ExpressionHelpers.ToNumber(_statementBuildParts.Pop());
            _statementBuildParts.Push(expr);
        }

        protected override void VisitForUpperLimit(BslSyntaxNode node)
        {
            base.VisitForUpperLimit(node);
            var limit = Expression.Convert(
                ExpressionHelpers.ToNumber(_statementBuildParts.Pop()),
                typeof(decimal));
            
            _blocks.GetCurrentBlock().BuildStack.Push(limit);
        }
        
        #endregion
        
        #region ForEach Loop
        
        protected override void VisitForEachLoopNode(ForEachLoopNode node)
        {
            _blocks.EnterBlock(new JumpInformationRecord
            {
                LoopBreak = Expression.Label(),
                LoopContinue = Expression.Label()
            });
            base.VisitForEachLoopNode(node);

            var block = _blocks.LeaveBlock();
            var enumerableCollection = block.BuildStack.Pop();
            var itemVariable = block.BuildStack.Pop();
            
            var collectionType = typeof(IEnumerable);
            var getEnumeratorMethod = collectionType.GetMethod("GetEnumerator");
            var moveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
            var collectionCast = Expression.Convert(enumerableCollection, collectionType);
            
            Debug.Assert(moveNextMethod != null);
            Debug.Assert(getEnumeratorMethod != null);
            
            // loop init section
            var getEnumeratorInvoke = Expression.Call(collectionCast, getEnumeratorMethod);
            var enumeratorVar = Expression.Variable(typeof(IEnumerator));
            
            var result = new List<Expression>();
            result.Add(Expression.Assign(enumeratorVar, getEnumeratorInvoke));
            
            
            var loop = new List<Expression>();

            var assignCurrent = Expression.Assign(
                itemVariable,
                Expression.Convert(
                    Expression.Property(enumeratorVar, "Current"),
                    typeof(BslValue))
            );
            
            loop.Add(assignCurrent);
            loop.AddRange(block.GetStatements());
            
            var finalLoop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.Equal(Expression.Call(enumeratorVar, moveNextMethod), Expression.Constant(true)),
                    Expression.Block(loop),
                    Expression.Break(block.LoopBreak)),
                block.LoopBreak, block.LoopContinue);
            
            result.Add(finalLoop);

            _blocks.Add(Expression.Block(new[] {enumeratorVar}, result));
        }
        
        protected override void VisitIteratorLoopVariable(TerminalNode node)
        {
            // temp var for VisitVariableWrite()
            _statementBuildParts.Push(Expression.Variable(typeof(BslValue)));
            base.VisitIteratorLoopVariable(node);
            // push variable
            _blocks.GetCurrentBlock().BuildStack.Push(_statementBuildParts.Pop());
            // clear temp var
            _statementBuildParts.Pop();
        }
        
        protected override void VisitIteratorExpression(BslSyntaxNode node)
        {
            base.VisitIteratorExpression(node);
            _blocks.GetCurrentBlock().BuildStack.Push(_statementBuildParts.Pop());
        }
        #endregion

        #region TryExcept Block
        
        protected override void VisitTryExceptNode(TryExceptNode node)
        {
            _blocks.EnterBlock(new JumpInformationRecord
            {
                ExceptionInfo = Expression.Parameter(typeof(Exception))
            });
            base.VisitTryExceptNode(node);
            
            // TODO доделать все переобертки RuntimeException для стековой машины и для нативной

            var block = _blocks.LeaveBlock();
            var except = block.BuildStack.Pop();
            var tryBlock = block.BuildStack.Pop();
            
            _blocks.Add(Expression.TryCatch(tryBlock,
                Expression.Catch(block.CurrentException, except))
            );
        }

        protected override void VisitTryBlock(CodeBatchNode node)
        {
            _blocks.EnterBlock();
            base.VisitTryBlock(node);
            var block = _blocks.LeaveBlock();
            
            _blocks.GetCurrentBlock().BuildStack.Push(Expression.Block(typeof(void),block.GetStatements()));
        }

        protected override void VisitExceptBlock(CodeBatchNode node)
        {
            _blocks.EnterBlock();
            base.VisitExceptBlock(node);
            var block = _blocks.LeaveBlock();
            
            _blocks.GetCurrentBlock().BuildStack.Push(Expression.Block(typeof(void),block.GetStatements()));
        }

        protected override void VisitRaiseNode(BslSyntaxNode node)
        {
            if (node.Children.Count == 0)
            {
                _blocks.Add(Expression.Rethrow());
            }
            else
            {
                VisitExpression(node.Children[0]);
                var raiseArgExpression = ExpressionHelpers.ToString(_statementBuildParts.Pop());

                var exceptionExpression = ExpressionHelpers.CallOfInstanceMethod(
                    Expression.Constant(ExceptionInfoFactory),
                    nameof(IExceptionInfoFactory.Raise),
                    raiseArgExpression);
                
                _blocks.Add(Expression.Throw(exceptionExpression));
            }
            base.VisitRaiseNode(node);
        }
        
        #endregion

        #region Method Calls

        protected override void VisitGlobalProcedureCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {   
                AddError(LocalizedErrors.UseBuiltInFunctionAsProcedure());
                return;
            }

            var expression = CreateMethodCall(node);
            _blocks.Add(expression);
        }

        protected override void VisitGlobalFunctionCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {
                _statementBuildParts.Push(CreateBuiltInFunctionCall(node));
                return;
            }

            var methodExists = Symbols.TryFindMethod(
                node.Identifier.GetIdentifier(), out var methodSymbol);
            
            if (methodExists && !methodSymbol.Method.IsFunction())
            {
                AddError(LocalizedErrors.UseProcAsFunction(), node.Location);
            }

            var expression = CreateMethodCall(node);
            _statementBuildParts.Push(expression);
        }

        protected override void VisitObjectProcedureCall(BslSyntaxNode node)
        {
            var target = _statementBuildParts.Pop();
            var call = (CallNode) node;

            var targetType = target.Type;
            var name = call.Identifier.GetIdentifier();
            if (targetType.IsObjectValue())
            {
                var methodInfo = FindMethodOfType(node, targetType, name);
                var args = PrepareCallArguments(call.ArgumentList, methodInfo.GetParameters());

                _blocks.Add(Expression.Call(target, methodInfo, args));
            }
            else if (targetType.IsValue() || target is DynamicExpression)
            {
                var args = new List<Expression>();
                args.Add(target);
                args.AddRange(PrepareDynamicCallArguments(call.ArgumentList));

                var csharpArgs = new List<CSharpArgumentInfo>();
                csharpArgs.Add(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default));
                csharpArgs.AddRange(args.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default)));
                
                var binder = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(
                    CSharpBinderFlags.InvokeSimpleName,
                    name,
                    null,
                    typeof(BslObjectValue),
                    csharpArgs);

                var objectExpr = Expression.Dynamic(binder, typeof(object), args); 
                _blocks.Add(ExpressionHelpers.ConvertToType(objectExpr, typeof(BslValue)));
            }
            else
            {
                AddError(NativeCompilerErrors.TypeIsNotAnObjectType(targetType), node.Location);
            }
        }

        private IEnumerable<Expression> PrepareDynamicCallArguments(BslSyntaxNode argList)
        {
            return argList.Children.Select(passedArg =>
                passedArg.Children.Count > 0
                    ? ConvertToExpressionTree(passedArg.Children[0])
                    : Expression.Constant(BslSkippedParameterValue.Instance));
        }

        protected override void VisitObjectFunctionCall(BslSyntaxNode node)
        {
            var target = _statementBuildParts.Pop();
            var call = (CallNode) node;

            var targetType = target.Type;
            var name = call.Identifier.GetIdentifier();
            if (targetType.IsObjectValue())
            {
                var methodInfo = FindMethodOfType(node, targetType, name);
                if (methodInfo.ReturnType == typeof(void))
                {
                    throw new NativeCompilerException(BilingualString.Localize(
                        $"Метод {targetType}.{name} не является функцией",
                        $"Method {targetType}.{name} is not a function"), ToCodePosition(node.Location));
                }
            
                var args = PrepareCallArguments(call.ArgumentList, methodInfo.GetParameters());
                _statementBuildParts.Push(Expression.Call(target, methodInfo, args));
            }
            else if (targetType.IsContext())
            {
                _statementBuildParts.Push(ExpressionHelpers.CallContextMethod(target, name, PrepareDynamicCallArguments(call.ArgumentList)));
            }
            else if (targetType.IsValue() || target is DynamicExpression)
            {
                var args = new List<Expression>();
                args.Add(target);
                args.AddRange(PrepareDynamicCallArguments(call.ArgumentList));

                var csharpArgs = new List<CSharpArgumentInfo>();
                csharpArgs.Add(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default));
                csharpArgs.AddRange(args.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default)));

                var binder = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(
                    CSharpBinderFlags.InvokeSimpleName,
                    name,
                    null,
                    typeof(BslObjectValue),
                    csharpArgs);

                var objectExpr = Expression.Dynamic(binder, typeof(object), args);
                _statementBuildParts.Push(ExpressionHelpers.ConvertToType(objectExpr, typeof(BslValue)));
            }
            else
            {
                AddError(NativeCompilerErrors.TypeIsNotAnObjectType(targetType), node.Location);
            }
        }

        private MethodInfo FindMethodOfType(BslSyntaxNode node, Type targetType, string name)
        {
            MethodInfo methodInfo;
            try
            {
                methodInfo = _methodsCache.GetOrAdd(targetType, name);
            }
            catch (InvalidOperationException)
            {
                throw new NativeCompilerException(BilingualString.Localize(
                    $"Метод {name} не определен для типа {targetType}",
                    $"Method {name} is not defined for type {targetType}"), ToCodePosition(node.Location));
            }

            return methodInfo;
        }

        private PropertyInfo TryFindPropertyOfType(BslSyntaxNode node, Type targetType, string name)
        {
            PropertyInfo propertyInfo;
            try
            {
                propertyInfo = _propertiesCache.GetOrAdd(targetType, name);
            }
            catch (InvalidOperationException)
            {
                propertyInfo = null;
            }

            return propertyInfo;
        }
        
        private PropertyInfo FindPropertyOfType(BslSyntaxNode node, Type targetType, string name)
        {
            PropertyInfo propertyInfo;
            try
            {
                propertyInfo = _propertiesCache.GetOrAdd(targetType, name);
            }
            catch (InvalidOperationException)
            {
                throw new NativeCompilerException(BilingualString.Localize(
                    $"Свойство {name} не определено для типа {targetType}",
                    $"Property {name} is not defined for type {targetType}"));
            }

            return propertyInfo;
        }

        private Expression CreateMethodCall(CallNode node)
        {
            if (!Symbols.TryFindMethodBinding(node.Identifier.GetIdentifier(), out var binding))
            {
                AddError($"Unknown method {node.Identifier.GetIdentifier()}", node.Location);
                return null;
            }

            var symbol = Symbols.GetScope(binding.ScopeNumber).Methods[binding.MemberNumber];
            var args = PrepareCallArguments(node.ArgumentList, symbol.Method.GetParameters());

            var methodInfo = symbol.Method;
            if (methodInfo is ContextMethodInfo contextMethod)
            {
                return DirectClrCall(
                    GetMethodBinding(binding, symbol),
                    contextMethod.GetWrappedMethod(),
                    args);
            }

            if (methodInfo is BslNativeMethodInfo nativeMethod)
            {
                return ExpressionHelpers.InvokeBslNativeMethod(
                    nativeMethod,
                    IsModuleScope(binding.ScopeNumber) ? _thisParameter : GetMethodBinding(binding, symbol),
                    args);
            }

            throw new InvalidOperationException($"Unknown method type {symbol.Method.GetType()}");

        }

        private Expression DirectClrCall(object target, MethodInfo clrMethod, IEnumerable<Expression> args)
        {
            return Expression.Call(Expression.Constant(target), clrMethod, args);   
        }
        
        private Expression CreateBuiltInFunctionCall(CallNode node)
        {
            Expression result;
            switch (node.Identifier.Lexem.Token)
            {
                case Token.Bool:
                    result = DirectConversionCall(node, typeof(bool));
                    break;
                case Token.Number:
                    result = DirectConversionCall(node, typeof(decimal));
                    break;
                case Token.Str:
                    result = DirectConversionCall(node, typeof(string));
                    break;
                case Token.Date:
                    result = DirectConversionCall(node, typeof(string));
                    break;
                case Token.Type:
                    CheckArgumentsCount(node.ArgumentList, 1);
                    result = ExpressionHelpers.TypeByNameCall(CurrentTypeManager,
                        ConvertToExpressionTree(node.ArgumentList.Children[0].Children[0]));
                    break;
                case Token.ExceptionInfo:
                    CheckArgumentsCount(node.ArgumentList, 0);
                    result = GetRuntimeExceptionObject();
                    
                    break;
                case Token.ExceptionDescr:
                    CheckArgumentsCount(node.ArgumentList, 0);
                    result = GetRuntimeExceptionDescription();
                    break;
                case Token.ModuleInfo:
                    var factory = _services.Resolve<IScriptInformationFactory>();
                    result = ExpressionHelpers.CallScriptInfo(factory, _source);
                    break;
                default:
                    var methodName = node.Identifier.GetIdentifier();
                    var method = _builtInFunctions.GetMethod(methodName);
                    var declaredParameters = method.GetParameters();

                    var args = PrepareCallArguments(node.ArgumentList, declaredParameters);

                    result = Expression.Call(method, args);
                    break;
            }

            return result;
        }

        private Expression GetRuntimeExceptionDescription()
        {
            var excVariable = _blocks.GetCurrentBlock().CurrentException;
            Expression factoryArgument;
            // нас вызвали вне попытки-исключения
            factoryArgument = excVariable == null ? Expression.Constant(null, typeof(IRuntimeContextInstance)) : GetRuntimeExceptionObject();

            var factory = Expression.Constant(ExceptionInfoFactory);
            return ExpressionHelpers.CallOfInstanceMethod(
                factory,
                nameof(IExceptionInfoFactory.GetExceptionDescription),
                factoryArgument);
        }

        private Expression GetRuntimeExceptionObject()
        {
            var excVariable = _blocks.GetCurrentBlock().CurrentException;
            Expression factoryArgument;
            if (excVariable == null)
            {
                // нас вызвали вне попытки-исключения
                factoryArgument = Expression.Constant(null, typeof(Exception));
            }
            else
            {
                factoryArgument = excVariable;
            }

            var factory = Expression.Constant(ExceptionInfoFactory);
            return ExpressionHelpers.CallOfInstanceMethod(
                factory,
                nameof(IExceptionInfoFactory.GetExceptionInfo),
                factoryArgument);
        }

        private void CheckArgumentsCount(BslSyntaxNode argList, int needed)
        {
            if (argList.Children.Count < needed)
            {
                var errText = new BilingualString("Недостаточно фактических параметров",
                    "Not enough actual parameters");
                AddError(errText, argList.Location);
            }

            if (argList.Children.Count > needed || argList.Children.Any(x => x.Children.Count == 0))
            {
                var errText = new BilingualString("Слишком много фактических параметров",
                    "Too many actual parameters");
                AddError(errText, argList.Location);
            }
        }

        private Expression DirectConversionCall(CallNode node, Type type)
        {
            CheckArgumentsCount(node.ArgumentList, 1);
            return ExpressionHelpers.ConvertToType(
                ConvertToExpressionTree(node.ArgumentList.Children[0].Children[0]),
                type);
        }

        private List<Expression> PrepareCallArguments(BslSyntaxNode argList, ParameterInfo[] declaredParameters)
        {
            var factArguments = new List<Expression>();

            var parameters = argList.Children.Select(passedArg =>
                passedArg.Children.Count > 0
                    ? ConvertToExpressionTree(passedArg.Children[0])
                    : null).ToArray();

            var parametersToProcess = declaredParameters.Length;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parametersToProcess == 0)
                {
                    var errText = new BilingualString("Слишком много фактических параметров",
                        "Too many actual parameters");
                    AddError(errText, argList.Location);
                }

                var passedArg = parameters[i];
                var declaredParam = declaredParameters[i];

                if (declaredParam.GetCustomAttribute<ParamArrayAttribute>() != null)
                {
                    // это спецпараметр
                    Debug.Assert(declaredParam.ParameterType.IsArray);
                    var arrayItemType = declaredParam.ParameterType.GetElementType();
                    Debug.Assert(arrayItemType != null);
                    var remainParameters = parameters
                        .Skip(i)
                        .Select(x => PassSingleParameter(x, arrayItemType, argList.Location));

                    var paramArray = Expression.NewArrayInit(arrayItemType, remainParameters);
                    factArguments.Add(paramArray);
                    
                    parametersToProcess = 0;
                    break;
                }
                else
                {
                    if (passedArg != null)
                    {
                        var convertedOrDirect = PassSingleParameter(passedArg, declaredParam.ParameterType, argList.Location);
                        factArguments.Add(convertedOrDirect);
                    }
                    else if (declaredParam.HasDefaultValue)
                    {
                        factArguments.Add(Expression.Constant(declaredParam.DefaultValue, declaredParam.ParameterType));
                    }
                    else
                    {
                        var errText = new BilingualString(
                            $"Пропущен обязательный параметр {declaredParam.Position+1} '{declaredParam.Name}'",
                            $"Missing mandatory parameter {declaredParam.Position+1} '{declaredParam.Name}'");
                        AddError(errText, argList.Location);
                    }
                }

                --parametersToProcess;
            }

            if (parametersToProcess > 0)
            {
                foreach (var declaredParam in declaredParameters.Skip(parameters.Length))
                {
                    if (declaredParam.HasDefaultValue)
                    {
                        factArguments.Add(Expression.Constant(declaredParam.DefaultValue, declaredParam.ParameterType));
                    }
                    else
                    {
                        var errText = new BilingualString("Недостаточно фактических параметров",
                            "Not enough actual parameters");
                        AddError(errText, argList.Location);
                    }
                }
            }

            return factArguments;
        }

        private Expression PassSingleParameter(Expression passedArg, Type targetType, CodeRange location)
        {
            if (passedArg == null)
            {
                return Expression.Constant(BslSkippedParameterValue.Instance);
            }

            var convertedOrDirect = ExpressionHelpers.TryConvertParameter(passedArg, targetType);
            if (convertedOrDirect == null)
            {
                AddError(
                    new BilingualString(
                        $"Не удается выполнить преобразование параметра из типа {passedArg.Type} в тип {targetType}",
                        $"Cannot convert parameter from type {passedArg.Type} to type {targetType}"),
                    location);
            }

            return convertedOrDirect;
        }

        #endregion

        #region Constructors

        protected override void VisitNewObjectCreation(NewObjectNode node)
        {
            var services = Expression.Constant(_services);
            
            Expression[] parameters;
            if (node.ConstructorArguments != default)
            {
                parameters = node.ConstructorArguments.Children.Select(passedArg => 
                    passedArg.Children.Count > 0 ? 
                        ConvertToExpressionTree(passedArg.Children[0]) :
                        Expression.Default(typeof(BslValue))).ToArray();
            }
            else
            {
                parameters = new Expression[0];
            }

            if (node.IsDynamic)
            {
                var typeName = ConvertToExpressionTree(node.TypeNameNode);
                var call = ExpressionHelpers.ConstructorCall(CurrentTypeManager, services, typeName, parameters);
                _statementBuildParts.Push(call);
            }
            else
            {
                var typeNameString = node.TypeNameNode.GetIdentifier();
                var isKnownType = CurrentTypeManager.TryGetType(typeNameString, out var typeDef);
                if (isKnownType)
                {
                    var call = ExpressionHelpers.ConstructorCall(CurrentTypeManager, services, typeDef, parameters);
                    _statementBuildParts.Push(call);
                }
                else // это может быть тип, подключенный через ПодключитьСценарий
                {
                    var typeName = Expression.Constant(typeNameString);
                    var call = ExpressionHelpers.ConstructorCall(CurrentTypeManager, services, typeName, parameters);
                    _statementBuildParts.Push(call);
                }
            }
            
        }
        
        #endregion
        
        private Expression ConvertToExpressionTree(BslSyntaxNode arg)
        {
            VisitExpression(arg);
            return _statementBuildParts.Pop();
        }
        
        protected override void AddError(BilingualString errorText, CodeRange location)
        {
            base.AddError(errorText, location);
            throw new InternalFlowInterruptException();
        }
    }
}