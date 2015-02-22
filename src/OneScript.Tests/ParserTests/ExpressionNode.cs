using OneScript.Compiler;
using System;

namespace OneScript.Tests
{
    class BinExpressionNode : TestASTNodeBase
    {
        public IASTNode left;
        public IASTNode right;
        public Token opCode;

        protected override bool EqualsInternal(IASTNode other)
        {
            var binExpr = other as BinExpressionNode;
            bool lefts = ((IEquatable<IASTNode>)left).Equals(binExpr.left);
            bool rights = ((IEquatable<IASTNode>)right).Equals(binExpr.right);
            return opCode == binExpr.opCode && lefts && rights;
        }
    }

    class UnaryExpressionNode : TestASTNodeBase
    {
        public IASTNode operand;
        public Token opCode;

        protected override bool EqualsInternal(IASTNode other)
        {
            var un = other as UnaryExpressionNode;
            return opCode == un.opCode && ((IEquatable<IASTNode>)operand).Equals(un.operand);
        }
    }

    class OperandNode : TestASTNodeBase
    {
        public OperandNode()
        {

        }

        public OperandNode(string content)
        {
            this.content = content;
        }

        public string content;
        protected override bool EqualsInternal(IASTNode other)
        {
            return (other as OperandNode).content == content;
        }
    }

}