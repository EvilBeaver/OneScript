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
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis.Traversal
{
    public class BslSyntaxWalker
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

            _nodeVisitors[NodeKind.Module] = VisitModule;
            _nodeVisitors[NodeKind.Assignment] = VisitAssignment;
            _nodeVisitors[NodeKind.DereferenceOperation] = VisitDereferenceOperation;
            _nodeVisitors[NodeKind.IndexAccess] = VisitIndexAccess;
            _nodeVisitors[NodeKind.GlobalCall] = VisitGlobalFunctionCall;
            _nodeVisitors[NodeKind.BinaryOperation] = VisitBinaryOperationInternal;
            _nodeVisitors[NodeKind.UnaryOperation] = VisitUnaryOperationInternal;

        }

        protected void SetDefaultVisitorFor(int kind, Action<BslSyntaxNode> action)
        {
            _nodeVisitors[kind] = action;
        }
        
        protected void ChangeVisitorsDispatch(IEnumerable<Action<BslSyntaxNode>> newVisitors)
        {
            _nodeVisitors = newVisitors.ToArray();
        }

        protected Action<BslSyntaxNode>[] GetVisitorsDispatch() => _nodeVisitors;
        
        protected virtual void VisitModule(BslSyntaxNode node)
        {
            foreach (var child in node.Children)
            {
                switch (child.Kind)
                {
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
                }
            }

        }

        protected virtual void VisitModuleVariable(VariableDefinitionNode varNode)
        {
        }

        protected virtual void VisitMethod(MethodNode methodNode)
        {
            VisitMethodSignature(methodNode.Signature);
            VisitMethodBody(methodNode);
        }

        protected virtual void VisitMethodSignature(MethodSignatureNode methodNodeSignature)
        {
        }
        
        protected virtual void VisitMethodBody(MethodNode methodNode)
        {
            foreach (var variableDefinition in methodNode.VariableDefinitions())
            {
                VisitMethodVariable(methodNode, variableDefinition);
            }

            VisitCodeBlock(methodNode.MethodBody);
        }

        protected virtual void VisitCodeBlock(BslSyntaxNode statements)
        {
            foreach (var statement in statements.Children)
            {
                VisitStatement(statement);
            }
        }

        protected virtual void VisitStatement(BslSyntaxNode statement)
        {
            if(statement.Kind == NodeKind.GlobalCall)
                VisitGlobalProcedureCall(statement);
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

        protected virtual void VisitGlobalFunctionCall(BslSyntaxNode node)
        {
        }
        
        protected virtual void VisitGlobalProcedureCall(BslSyntaxNode node)
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
        
        private void VisitBinaryOperationInternal(BslSyntaxNode node)
        {
            VisitBinaryOperation((BinaryOperationNode)node);
        }
        
        private void VisitUnaryOperationInternal(BslSyntaxNode node)
        {
            VisitUnaryOperation((UnaryOperationNode)node);
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
            VisitCodeBlock(codeBlock);
        }

        public void Visit(BslSyntaxNode node)
        {
            DefaultVisit(node);
        }

        protected virtual void DefaultVisit(BslSyntaxNode node)
        {
            var action = _nodeVisitors[node.Kind];
            action?.Invoke(node);
        }
    }
}