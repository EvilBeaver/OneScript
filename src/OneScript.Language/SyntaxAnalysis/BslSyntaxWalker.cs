/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public abstract class BslSyntaxWalker
    {
        private Action<BslSyntaxNode>[] _nodeVisitors;

        public BslSyntaxWalker()
        {
            CreateVisitors();
        }

        private void CreateVisitors()
        {
            _nodeVisitors = new Action<BslSyntaxNode>[
                typeof(NodeKind).GetFields(BindingFlags.Static|BindingFlags.Public).Length
            ];

            _nodeVisitors[(int)NodeKind.Module] = x => VisitModule((ModuleNode)x);
            _nodeVisitors[(int)NodeKind.Assignment] = VisitAssignment;
            _nodeVisitors[(int)NodeKind.DereferenceOperation] = VisitDereferenceOperation;
            _nodeVisitors[(int)NodeKind.IndexAccess] = VisitIndexAccess;
            _nodeVisitors[(int)NodeKind.GlobalCall] = (x) => VisitGlobalFunctionCall((CallNode)x);
            _nodeVisitors[(int)NodeKind.BinaryOperation] = (x) => VisitBinaryOperation((BinaryOperationNode)x);
            _nodeVisitors[(int)NodeKind.UnaryOperation] = (x) => VisitUnaryOperation((UnaryOperationNode)x);
            _nodeVisitors[(int)NodeKind.TernaryOperator] = VisitTernaryOperation;
            _nodeVisitors[(int)NodeKind.WhileLoop] = (x) => VisitWhileNode((WhileLoopNode)x);
            _nodeVisitors[(int)NodeKind.Condition] = (x) => VisitIfNode((ConditionNode)x);
            _nodeVisitors[(int)NodeKind.ForEachLoop] = (x) => VisitForEachLoopNode((ForEachLoopNode)x);
            _nodeVisitors[(int)NodeKind.ForLoop] = (x) => VisitForLoopNode((ForLoopNode)x);
            _nodeVisitors[(int)NodeKind.BreakStatement] = (x) => VisitBreakNode((LineMarkerNode)x);
            _nodeVisitors[(int)NodeKind.ContinueStatement] = (x) => VisitContinueNode((LineMarkerNode)x);
            _nodeVisitors[(int)NodeKind.ReturnStatement] = VisitReturnNode;
            _nodeVisitors[(int)NodeKind.RaiseException] = VisitRaiseNode;
            _nodeVisitors[(int)NodeKind.TryExcept] = (x) => VisitTryExceptNode((TryExceptNode)x);
            _nodeVisitors[(int)NodeKind.ExecuteStatement] = VisitExecuteStatement;
            _nodeVisitors[(int)NodeKind.AddHandler] = VisitHandlerOperation;
            _nodeVisitors[(int)NodeKind.RemoveHandler] = VisitHandlerOperation;
            _nodeVisitors[(int)NodeKind.NewObject] = (x) => VisitNewObjectCreation((NewObjectNode)x);
            _nodeVisitors[(int)NodeKind.Preprocessor] = (x) => VisitPreprocessorDirective((PreprocessorDirectiveNode)x);

        }

        protected void SetDefaultVisitorFor(NodeKind kind, Action<BslSyntaxNode> action)
        {
            _nodeVisitors[(int)kind] = action;
        }
        
        protected void ChangeVisitorsDispatch(IEnumerable<Action<BslSyntaxNode>> newVisitors)
        {
            _nodeVisitors = newVisitors.ToArray();
        }

        protected Action<BslSyntaxNode>[] GetVisitorsDispatch() => _nodeVisitors;
        
        protected virtual void VisitModule(ModuleNode node)
        {
            foreach (var child in node.Children)
            {
                switch (child.Kind)
                {
                    case NodeKind.Annotation:
                    case NodeKind.Import:
                        VisitModuleAnnotation((AnnotationNode) child);
                        break;
                    case NodeKind.VariablesSection:
                        foreach (var varNode in child.Children)
                        {
                            VisitModuleVariable((VariableDefinitionNode) varNode);
                        }
                        break;
                    case NodeKind.MethodsSection:
                        foreach (var methodNode in child.Children)
                        {
                            VisitMethod((MethodNode) methodNode);
                        }
                        break;
                    case NodeKind.ModuleBody:
                        VisitModuleBody(child);
                        break;
                    case NodeKind.TopLevelExpression:
                        var expression = child.Children.FirstOrDefault();
                        if(expression != default)
                            VisitExpression(expression);
                        
                        break;
                    default:
                        Visit(child);
                        break;
                }
            }

        }

        protected virtual void VisitModuleAnnotation(AnnotationNode node)
        {
        }

        protected virtual void VisitModuleVariable(VariableDefinitionNode varNode)
        {
        }

        protected virtual void VisitMethod(MethodNode methodNode)
        {
            VisitMethodSignature(methodNode.Signature);
            VisitMethodBody(methodNode);
            VisitBlockEnd(methodNode.EndLocation);
        }

        protected virtual void VisitBlockEnd(in CodeRange endLocation)
        {
        }

        protected virtual void VisitMethodSignature(MethodSignatureNode node)
        {
        }
        
        protected virtual void VisitMethodBody(MethodNode methodNode)
        {
            foreach (var variableDefinition in methodNode.VariableDefinitions())
            {
                VisitMethodVariable(methodNode, variableDefinition);
            }

            VisitCodeBlock(methodNode.MethodBody);
            VisitBlockEnd(methodNode.EndLocation);
        }

        protected virtual void VisitCodeBlock(CodeBatchNode statements)
        {
            foreach (var statement in statements.Children)
            {
                VisitStatement(statement);
            }
        }

        protected void VisitCodeBlock(BslSyntaxNode node)
        {
            VisitCodeBlock((CodeBatchNode)node);
        }

        protected virtual void VisitStatement(BslSyntaxNode statement)
        {
            if(statement.Kind == NodeKind.GlobalCall)
                VisitGlobalProcedureCall(statement as CallNode);
            else if (statement.Kind == NodeKind.DereferenceOperation)
                VisitProcedureDereference(statement);
            else
                DefaultVisit(statement);
        }

        protected virtual void VisitAssignment(BslSyntaxNode assignment)
        {
            var left = assignment.Children[0];
            var right = assignment.Children[1];
            VisitAssignmentLeftPart(left);
            VisitAssignmentRightPart(right);
        }

        protected virtual void VisitAssignmentLeftPart(BslSyntaxNode node)
        {
        }
        
        protected virtual void VisitAssignmentRightPart(BslSyntaxNode node)
        {
            VisitExpression(node);
        }

        protected virtual void VisitMethodVariable(MethodNode method, VariableDefinitionNode variableDefinition)
        {
        }

        protected virtual void VisitReferenceRead(BslSyntaxNode node)
        {
            DefaultVisit(node);
        }
        
        protected virtual void VisitReferenceWrite(BslSyntaxNode node)
        {
        }
        
        protected virtual void VisitDereferenceOperation(BslSyntaxNode node)
        {
            var target = node.Children[0];
            var operand = node.Children[1];
            VisitAccessTarget(target);
            VisitDereferenceOperand(operand);
        }

        protected virtual void VisitIndexAccess(BslSyntaxNode node)
        {
            var target = node.Children[0];
            var operand = node.Children[1];
            VisitAccessTarget(target);
            VisitIndexExpression(operand);
        }

        protected virtual void VisitIndexExpression(BslSyntaxNode operand)
        {
            VisitExpression(operand);
        }

        protected virtual void VisitExpression(BslSyntaxNode expression)
        {
            if (expression is TerminalNode term)
            {
                if (term.Kind == NodeKind.Identifier)
                    VisitVariableRead(term);
                else
                    VisitConstant(term);
            }
            else
            {
                DefaultVisit(expression);
            }
        }

        protected virtual void VisitConstant(TerminalNode node)
        {
        }

        protected virtual void VisitGlobalFunctionCall(CallNode node)
        {
        }
        
        protected virtual void VisitGlobalProcedureCall(CallNode node)
        {
        }
        
        protected virtual void VisitProcedureDereference(BslSyntaxNode statement)
        {
            VisitAccessTarget(statement.Children[0]);
            VisitObjectProcedureCall(statement.Children[1]);
        }

        protected virtual void VisitObjectProcedureCall(BslSyntaxNode node)
        {
        }

        protected virtual void VisitDereferenceOperand(BslSyntaxNode operand)
        {
            if (operand.Kind == NodeKind.Identifier)
            {
                VisitResolveProperty((TerminalNode) operand);
            }
            else
            {
                VisitObjectFunctionCall(operand);
            }
        }

        protected virtual void VisitObjectFunctionCall(BslSyntaxNode node)
        {
        }

        protected virtual void VisitResolveProperty(TerminalNode operand)
        {
        }

        protected virtual void VisitAccessTarget(BslSyntaxNode node)
        {
            if (node.Kind == NodeKind.Identifier)
            {
                VisitVariableRead((TerminalNode) node);
            }
            else
            {
                VisitReferenceRead(node);
            }
        }

        protected virtual void VisitVariableRead(TerminalNode node)
        {
        }
        
        protected virtual void VisitVariableWrite(TerminalNode node)
        {
        }
        
        protected virtual void VisitBinaryOperation(BinaryOperationNode node)
        {
        }
        
        protected virtual void VisitUnaryOperation(UnaryOperationNode node)
        {
        }
        
        protected virtual void VisitTernaryOperation(BslSyntaxNode node)
        {
        }

        protected virtual void VisitModuleBody(BslSyntaxNode codeBlock)
        {
            VisitCodeBlock(codeBlock.Children[0]);
        }

        protected virtual void VisitWhileNode(WhileLoopNode node)
        {
            VisitWhileCondition(node.Children[0]);
            VisitWhileBody(node.Children[1]);
            VisitBlockEnd(node.EndLocation);
        }

        protected virtual void VisitWhileBody(BslSyntaxNode node)
        {
            VisitCodeBlock(node);
        }

        protected virtual void VisitWhileCondition(BslSyntaxNode node)
        {
            VisitExpression(node);
        }

        protected virtual void VisitIfNode(ConditionNode node)
        {
            VisitIfExpression(node.Expression);
            VisitIfTruePart(node.TruePart);
            foreach (var alternative in node.GetAlternatives())
            {
                VisitIfAlternative(alternative);
            }
            VisitBlockEnd(node.EndLocation);
        }

        protected virtual void VisitIfExpression(BslSyntaxNode node)
        {
            VisitExpression(node);
        }

        protected virtual void VisitIfTruePart(CodeBatchNode node)
        {
            VisitCodeBlock(node);
        }

        protected virtual void VisitIfAlternative(BslSyntaxNode node)
        {
            if (node.Kind == NodeKind.Condition)
            {
                VisitElseIfNode((ConditionNode)node);
            }
            else
            {
                VisitElseNode((CodeBatchNode)node);
            }
        }

        protected virtual void VisitElseIfNode(ConditionNode node)
        {
            VisitIfExpression(node.Expression);
            VisitIfTruePart(node.TruePart);
        }
        
        protected virtual void VisitElseNode(CodeBatchNode node)
        {
            VisitCodeBlock(node);
        }
        
        protected virtual void VisitForEachLoopNode(ForEachLoopNode node)
        {
            VisitIteratorLoopVariable(node.IteratorVariable);
            VisitIteratorExpression(node.CollectionExpression);
            VisitIteratorLoopBody(node.LoopBody);
            VisitBlockEnd(node.EndLocation);
        }

        protected virtual void VisitIteratorLoopVariable(TerminalNode node)
        {
            VisitVariableWrite(node);
        }

        protected virtual void VisitIteratorExpression(BslSyntaxNode node)
        {
            VisitExpression(node);
        }
        
        protected virtual void VisitIteratorLoopBody(BslSyntaxNode node)
        {
            VisitCodeBlock(node);
        }

        protected virtual void VisitForLoopNode(ForLoopNode node)
        {
            VisitForInitializer(node.InitializationClause);
            VisitForUpperLimit(node.UpperLimitExpression);
            VisitForLoopBody(node.LoopBody);
            VisitBlockEnd(node.EndLocation);
        }

        protected virtual void VisitForInitializer(BslSyntaxNode node)
        {
            var forLoopIterator = node.Children[0];
            var forLoopInitialValue = node.Children[1];
            VisitForLoopIterator(forLoopIterator);
            VisitForLoopInitialValue(forLoopInitialValue);
        }

        protected virtual void VisitForLoopIterator(BslSyntaxNode node)
        {
            VisitAssignmentLeftPart(node);
        }

        protected virtual void VisitForLoopInitialValue(BslSyntaxNode node)
        {
            VisitExpression(node);
        }

        protected virtual void VisitForUpperLimit(BslSyntaxNode node)
        {
            VisitExpression(node);
        }

        protected virtual void VisitForLoopBody(CodeBatchNode node)
        {
            VisitCodeBlock(node);
        }

        protected virtual void VisitBreakNode(LineMarkerNode node)
        {
        }
        
        protected virtual void VisitContinueNode(LineMarkerNode node)
        {
        }
        
        protected virtual void VisitReturnNode(BslSyntaxNode node)
        {
        }

        protected virtual void VisitRaiseNode(BslSyntaxNode node)
        {
        }
        
        protected virtual void VisitTryExceptNode(TryExceptNode node)
        {
            VisitTryBlock(node.TryBlock);
            VisitExceptBlock(node.TryBlock);
            VisitBlockEnd(node.EndLocation);
        }

        protected virtual void VisitTryBlock(CodeBatchNode node)
        {
            VisitCodeBlock(node);
        }

        protected virtual void VisitExceptBlock(CodeBatchNode node)
        {
            VisitCodeBlock(node);
        }

        protected virtual void VisitExecuteStatement(BslSyntaxNode node)
        {
            VisitExpression(node.Children[0]);
        }

        protected virtual void VisitHandlerOperation(BslSyntaxNode node)
        {
        }
        
        protected virtual void VisitNewObjectCreation(NewObjectNode node)
        {
        }
        
        protected virtual void VisitPreprocessorDirective(PreprocessorDirectiveNode node)
        {
        }
        
        public void Visit(BslSyntaxNode node)
        {
            DefaultVisit(node);
        }

        protected virtual void DefaultVisit(BslSyntaxNode node)
        {
            var action = _nodeVisitors[(int)node.Kind];
            action?.Invoke(node);
        }
    }
}