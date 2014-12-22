using OneScript.Scripting.Compiler;

namespace OneScript.Tests
{
    class AssignmentNode : TestASTNodeBase
    {
        public IASTNode Left;
        public IASTNode Right;

        public AssignmentNode(IASTNode left, IASTNode right)
        {
            Left = left;
            Right = right;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var left = (TestASTNodeBase)Left;
            var right = (TestASTNodeBase)Right;

            var otherTest = (AssignmentNode)other;

            return left.Equals(otherTest.Left) && right.Equals(otherTest.Right);
        }
    }
}