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
using OneScript.Commons;
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
            block.Add(Expression.Label(
                block.MethodReturn, 
                Expression.Constant(BslUndefinedValue.Instance)));
            
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
                if (e is BslCoreException)
                    throw;

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
            }
            
            var right = _statementBuildParts.Pop();
            _blocks.Add(MakeAssign(left, right));
        }

        private Expression AssignToDynamicObject(Expression left, Expression right)
        {
            throw new NotImplementedException();
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
                VisitReferenceRead(node);
            }
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

            indexExpression ??= ExpressionHelpers.DynamicGetIndex(target, index);
            
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
                indexExpression = ExpressionHelpers.DynamicSetIndex(target, index, value);
            }
            
            _statementBuildParts.Push(indexExpression);
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
            catch (CompilerException e)
            {
                AddError(e.Message, binaryOperationNode.Location);
                return null;
            }
        }
        
        private Expression MakeAssign(Expression left, Expression right)
        {
            if (!left.Type.IsAssignableFrom(right.Type))
            {
                right = ExpressionHelpers.ConvertToType(right, left.Type);
            }
            
            return Expression.Assign(left, right);
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
            _blocks.EnterBlock();
            base.VisitTryExceptNode(node);
            
            // TODO доделать все переобертки RuntimeException для стековой машины и для нативной

            var block = _blocks.LeaveBlock();
            var except = block.BuildStack.Pop();
            var tryBlock = block.BuildStack.Pop();
            
            _blocks.Add(Expression.TryCatch(tryBlock,
                Expression.Catch(typeof(Exception), except))
            );
        }

        protected override void VisitTryBlock(CodeBatchNode node)
        {
            _blocks.EnterBlock();
            base.VisitTryBlock(node);
            var block = _blocks.LeaveBlock();
            
            _blocks.GetCurrentBlock().BuildStack.Push(Expression.Block(block.GetStatements()));
        }

        protected override void VisitExceptBlock(CodeBatchNode node)
        {
            _blocks.EnterBlock();
            base.VisitExceptBlock(node);
            var block = _blocks.LeaveBlock();
            
            _blocks.GetCurrentBlock().BuildStack.Push(Expression.Block(block.GetStatements()));
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
                
                var exceptionType = typeof(BslRuntimeException);
                var ctor = exceptionType.GetConstructor(new Type[] {typeof(BilingualString)});
                var exceptionExpression = Expression.New(ctor, Expression.Convert(expression, typeof(BilingualString)));
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
            var expression = CreateMethodCall(node);
            _statementBuildParts.Push(expression);
        }

        private Expression CreateMethodCall(CallNode node)
        {
            if (!Symbols.FindMethod(node.Identifier.GetIdentifier(), out var binding))
            {
                AddError($"Unknown method {node.Identifier.GetIdentifier()}", node.Location);
                return null;
            }

            var symbol = Symbols.GetScope(binding.ScopeNumber).Methods[binding.MemberNumber];
            var args = node.ArgumentList.Children.Select(ConvertToExpressionTree);

            var context = Expression.Constant(symbol.Target);
            return Expression.Call(context, symbol.MethodInfo, args);
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