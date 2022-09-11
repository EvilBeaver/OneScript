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
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Localization;
using OneScript.Native.Runtime;
using OneScript.Runtime.Binding;
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
        
        private BinaryOperationCompiler _binaryOperationCompiler = new BinaryOperationCompiler();
        private ITypeManager _typeManager;
        private readonly IServiceContainer _services;
        private readonly BuiltInFunctionsCache _builtInFunctions = new BuiltInFunctionsCache();

        private ContextMethodsCache _methodsCache = new ContextMethodsCache();
        private ReflectedPropertiesCache _propertiesCache = new ReflectedPropertiesCache();

        public MethodCompiler(BslWalkerContext walkContext, BslNativeMethodInfo method) : base(walkContext)
        {
            _method = method;
            _services = walkContext.Services;
        }

        public ITypeManager CurrentTypeManager
        {
            get
            {
                if (_typeManager == default)
                {
                    _typeManager = _services.TryResolve<ITypeManager>();
                    if (_typeManager == default)
                        throw new NotSupportedException("Type manager is not registered in services.");
                }

                return _typeManager;
            }
        }
        
        public void CompileMethod(MethodNode methodNode)
        {
            _localVariables.AddRange(
                _method.GetParameters()
                    .Select(x => Expression.Parameter(x.ParameterType, x.Name)));
            
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
            Symbols.PushScope(new SymbolScope());
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
            
            var parameters = _localVariables.Take(_declaredParameters.Length).ToArray();

            var blockVariables = _localVariables.Skip(parameters.Length);
            
            var body = Expression.Block(
                blockVariables.ToArray(),
                block.GetStatements());

            var impl = Expression.Lambda(body, parameters);
            
            _method.SetImplementation(impl);
        }

        private void FillParameterVariables()
        {
            _declaredParameters = _method.GetBslParameters();
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
            catch (Exception e)
            {
                RestoreNestingLevel(nestingLevel);
                if (e is NativeCompilerException ce)
                {
                    ce.Position = ToCodePosition(statement.Location);
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
                AddError($"Unknown variable {node.GetIdentifier()}", node.Location);
                return;
            }

            var symbol = Symbols.GetScope(binding.ScopeNumber).Variables[binding.MemberNumber];
            if (symbol is IPropertySymbol && symbol is IBoundSymbol boundSymbol)
            {
                // prop read
                var target = boundSymbol.Target;
                _statementBuildParts.Push(Expression.Constant(target));
            }
            else
            {
                // local read
                var expr = _localVariables[binding.MemberNumber];
                _statementBuildParts.Push(expr);
            }
        }

        private void MakeReadPropertyAccess(TerminalNode operand)
        {
            var memberName = operand.Lexem.Content;
            var instance = _statementBuildParts.Pop();
            var instanceType = instance.Type;
            var prop = FindPropertyOfType(operand, instanceType, memberName);

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

            var args = new List<Expression>();
            args.Add(instance);
            var csharpArgs = new List<CSharpArgumentInfo>();
            csharpArgs.Add(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default));
            csharpArgs.AddRange(args.Select(x =>
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, default)));

            var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                CSharpBinderFlags.InvokeSimpleName,
                memberName,
                typeof(BslObjectValue), csharpArgs);
            var expr = Expression.Dynamic(binder, typeof(object), args);
            _statementBuildParts.Push((expr));
        }

        private void MakeWritePropertyAccess(TerminalNode operand)
        {
            var memberName = operand.Lexem.Content;
            var instance = _statementBuildParts.Pop();
            var instanceType = instance.Type;
            var prop = FindPropertyOfType(operand, instanceType, memberName);

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
            _statementBuildParts.Push((expr));
        }

        protected override void VisitResolveProperty(TerminalNode operand)
        {
            MakeReadPropertyAccess(operand);
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = Symbols.FindVariable(identifier, out var varBinding);
            if (hasVar)
            {
                var symbol = Symbols.GetScope(varBinding.ScopeNumber).Variables[varBinding.MemberNumber];
                if (!(symbol is PropertySymbol propSymbol))
                {
                    var local = _localVariables[varBinding.MemberNumber];
                    _statementBuildParts.Push(local);
                }
                else
                {
                    var convert = Expression.Convert(Expression.Constant(propSymbol.Target),
                            propSymbol.Target.GetType());
                    
                   var accessExpression = Expression.Property(convert, propSymbol.Property.SetMethod);
                   _statementBuildParts.Push(accessExpression);
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
                                  ?? ExpressionHelpers.DynamicGetIndex(target, index);
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
                                  ?? ExpressionHelpers.DynamicSetIndex(target, index, value);
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
                AddError($"Type {target.Type} is not indexed", node.Location);
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
            Debug.Assert(node.Children.Count > 0);
            
            VisitExpression(node.Children[0]);

            var resultExpr = _statementBuildParts.Pop();

            var label = _blocks.GetCurrentBlock().MethodReturn;
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
            result.Add(Expression.Assign(counterVar, initialValue)); // TODO: MakeAssign ?
            var finalVar = Expression.Variable(typeof(decimal)); // TODO: BslNumericValue ?
            result.Add(Expression.Assign(finalVar, upperLimit));
            
            var loop = new List<Expression>();
            loop.Add(Expression.IfThen(
                Expression.GreaterThan(counterVar, finalVar), 
                Expression.Break(block.LoopBreak)));
            
            loop.AddRange(block.GetStatements());
            
            loop.Add(Expression.Label(block.LoopContinue));
            loop.Add(Expression.PreIncrementAssign(counterVar));
            
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
                var expression = Expression.Call(
                    _statementBuildParts.Pop(),
                    typeof(object).GetMethod("ToString"));
                
                var exceptionType = typeof(RuntimeException);
                var ctor = exceptionType.GetConstructor(new Type[] {typeof(BilingualString)});
                var exceptionExpression = Expression.New(
                    ctor, 
                    Expression.Convert(expression, typeof(BilingualString)));
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
                // TODO поменять на BilingualString
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
            else if (targetType.IsValue())
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
                AddError(new BilingualString($"Тип {targetType} не является объектным типом.",$"Type {targetType} is not an object type."), node.Location);
            }
        }

        private IEnumerable<Expression> PrepareDynamicCallArguments(BslSyntaxNode argList)
        {
            return argList.Children.Select(passedArg =>
                passedArg.Children.Count > 0
                    ? ConvertToExpressionTree(passedArg.Children[0])
                    : null);
        }

        protected override void VisitObjectFunctionCall(BslSyntaxNode node)
        {
            var target = _statementBuildParts.Pop();
            var call = (CallNode) node;

            var targetType = target.Type;
            var name = call.Identifier.GetIdentifier();

            var methodInfo = FindMethodOfType(node, targetType, name);

            if (methodInfo.ReturnType == typeof(void))
            {
                throw new NativeCompilerException(new BilingualString(
                    $"Метод {targetType}.{name} не является функцией",
                    $"Method {targetType}.{name} is not a function"), ToCodePosition(node.Location));
            }
            
            var args = PrepareCallArguments(call.ArgumentList, methodInfo.GetParameters());
            
            _statementBuildParts.Push(Expression.Call(target, methodInfo, args));
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
                throw new NativeCompilerException(new BilingualString(
                    $"Метод {name} не определен для типа {targetType}",
                    $"Method {name} is not defined for type {targetType}"), ToCodePosition(node.Location));
            }

            return methodInfo;
        }

        private PropertyInfo FindPropertyOfType(BslSyntaxNode node, Type targetType, string name)
        {
            var props = targetType.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Public | BindingFlags.Instance,
                (info, criteria) =>
                {
                    var a = info.CustomAttributes.FirstOrDefault(data =>
                        data.AttributeType == typeof(ContextPropertyAttribute));
                    if (a == null) return false;
                    return a.ConstructorArguments.Any(alias =>
                        StringComparer.CurrentCultureIgnoreCase.Equals(alias.Value.ToString(), name));
                },
                null);

            if (props.Length != 1)
            {
                return null;
            }

            return props[0] as PropertyInfo;
        }

        private Expression CreateMethodCall(CallNode node)
        {
            if (!Symbols.FindMethod(node.Identifier.GetIdentifier(), out var binding))
            {
                AddError($"Unknown method {node.Identifier.GetIdentifier()}", node.Location);
                return null;
            }

            var symbol = Symbols.GetScope(binding.ScopeNumber).Methods[binding.MemberNumber];
            var args = PrepareCallArguments(node.ArgumentList, symbol.Method.GetParameters());

            var target = ((IBoundSymbol)symbol).Target;

            Expression context = target switch
            {
                ParameterExpression pe => pe,
                _ => Expression.Constant(target)
            };
            
            return Expression.Call(context, symbol.Method, args);
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
                        ConvertToExpressionTree(node.ArgumentList.Children[0]));
                    break;
                case Token.ExceptionInfo:
                    CheckArgumentsCount(node.ArgumentList, 0);
                    result = GetRuntimeExceptionObject();
                    
                    break;
                case Token.ExceptionDescr:
                    CheckArgumentsCount(node.ArgumentList, 0);
                    result = GetRuntimeExceptionDescription();
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
            var excInfo = GetRuntimeExceptionObject();
            if (excInfo.Type == typeof(BslUndefinedValue))
                return excInfo;
            
            return Expression.Property(excInfo, nameof(ExceptionInfoClass.Description));
        }

        private Expression GetRuntimeExceptionObject()
        {
            var excVariable = _blocks.GetCurrentBlock().CurrentException;
            if (excVariable == null)
                return Expression.Constant(BslUndefinedValue.Instance);
            
            return ExpressionHelpers.GetExceptionInfo(excVariable);
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

        private IEnumerable<Expression> PrepareCallArguments(BslSyntaxNode argList, ParameterInfo[] declaredParameters)
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
                    var remainParameters = parameters
                        .Skip(i)
                        .Select(x => PassSingleParameter(x, declaredParam.ParameterType, argList.Location));

                    var paramArray = Expression.NewArrayInit(declaredParam.ParameterType, remainParameters);
                    factArguments.Add(paramArray);
                    
                    parametersToProcess = 0;
                    break;
                }
                else
                {
                    var convertedOrDirect = PassSingleParameter(passedArg, declaredParam.ParameterType, argList.Location);
                    factArguments.Add(convertedOrDirect);
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
                return Expression.Default(targetType);
            }
            
            var convertedOrDirect = ExpressionHelpers.TryConvertParameter(passedArg, targetType);
            if (convertedOrDirect == null)
            {
                AddError(
                    new BilingualString(
                        $"Не удается выполнить преобразование из типа {passedArg.Type} в тип {targetType}"),
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
                var typeDef = CurrentTypeManager.GetTypeByName(typeNameString);
                var call = ExpressionHelpers.ConstructorCall(CurrentTypeManager, services, typeDef, parameters);
                _statementBuildParts.Push(call);
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