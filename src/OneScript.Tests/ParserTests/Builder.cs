using OneScript.Language;
using System;
using System.Collections.Generic;

namespace OneScript.Tests
{
    class Builder : IASTBuilder
    {

        List<string> _variables = new List<string>();
        CodeBatchNode _mainCode = new CodeBatchNode();
        List<MethodNode> _methods = new List<MethodNode>();

        Stack<CodeBatchNode> _batches = new Stack<CodeBatchNode>();

        public IASTNode topNode
        {
            get
            {
                if (_mainCode.Children.Count > 0)
                    return _mainCode.Children[0];
                else
                    return null;
            }
        }

        public List<string> Variables
        {
            get
            {
                return _variables;
            }
        }

        public CodeBatchNode CodeNode
        {
            get
            {
                if (_batches.Count == 0)
                    return _mainCode;

                return _batches.Peek();
            }
        }

        public List<MethodNode> Methods
        {
            get { return _methods; }
        }

        public void BeginModule()
        {
            _variables.Clear();
            _mainCode.Children.Clear();
            _batches.Clear();
            _methods.Clear();
        }

        public void CompleteModule()
        {
            //throw new NotImplementedException();
        }

        public void DefineExportVariable(string symbolicName)
        {
            _variables.Add(symbolicName + " export");
        }

        public void DefineVariable(string symbolicName)
        {
            _variables.Add(symbolicName);
        }

        public IASTNode SelectOrUseVariable(string identifier)
        {
            return ReadVariable(identifier);
        }

        public IASTNode BuildAssignment(IASTNode acceptor, IASTNode source)
        {
            var node = new AssignmentNode(acceptor, source);
            CodeNode.Add(node);

            return node;
        }

        public IASTNode ReadLiteral(Lexem lexem)
        {
            return new OperandNode()
            {
                content = lexem.Content
            };
        }

        public IASTNode ReadVariable(string identifier)
        {
            return new OperandNode()
            {
                content = identifier
            };
        }

        public IASTNode BinaryOperation(Token operationToken, IASTNode leftHandedNode, IASTNode rightHandedNode)
        {
            return new BinExpressionNode()
            {
                opCode = operationToken,
                left = leftHandedNode,
                right = rightHandedNode
            };
        }


        public IASTNode UnaryOperation(Token token, IASTNode operandNode)
        {
            if (token == Token.Minus || token == Token.UnaryMinus)
                token = Token.UnaryMinus;
            else if (token != Token.Not)
                throw new ArgumentException();

            return new UnaryExpressionNode() { opCode = token, operand = operandNode };

        }

        public IASTNode BuildFunctionCall(IASTNode target, string identifier, IASTNode[] args)
        {
            return new CallNode(identifier, args);
        }


        public IASTNode ResolveProperty(IASTNode target, string p)
        {
            return new PropertyAccessNode(target, p);
        }

        public IASTNode BuildIndexedAccess(IASTNode target, IASTNode expr)
        {
            return new IndexedAccessNode(target, expr);
        }


        public void BuildProcedureCall(IASTNode resolved, string ident, IASTNode[] args)
        {
            throw new NotImplementedException();
        }


        public IASTNode BeginMethod(string identifier, bool isFunction)
        {
            var methodNode = new MethodNode(identifier, isFunction);
            return methodNode;
        }

        public void SetMethodSignature(IASTNode methodNode, ASTMethodParameter[] parameters, bool isExported)
        {
            var method = methodNode as MethodNode;
            method._parameters = parameters;
            method._isExported = isExported;

        }

        public void EndMethod(IASTNode methodNode)
        {
            _methods.Add((MethodNode)methodNode);
        }

        public IASTNode BeginBatch()
        {
            var node = new CodeBatchNode();
            _batches.Push(node);
            return node;
        }

        public void EndBatch(IASTNode batch)
        {
            var node = _batches.Pop();
            if (_batches.Count == 0)
            {
                foreach (var child in node.Children)
                {
                    _mainCode.Children.Add(child);
                }
            }
        }

        public IASTConditionNode BeginConditionStatement()
        {
            return new ConditionNode();
        }

        public void EndConditionStatement(IASTConditionNode node)
        {
            CodeNode.Children.Add((TestASTNodeBase)node);
        }
    }
}