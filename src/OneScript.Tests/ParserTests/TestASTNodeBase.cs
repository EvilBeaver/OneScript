using OneScript.Scripting.Compiler;
using System;

namespace OneScript.Tests
{
    abstract class TestASTNodeBase : IASTNode, IEquatable<IASTNode>
    {
        public bool Equals(IASTNode other)
        {
            if (other == null)
                return false;

            if (other.GetType() == this.GetType())
            {
                return EqualsInternal(other);
            }
            else
            {
                return false;
            }
        }

        protected abstract bool EqualsInternal(IASTNode other);

        public static bool CompareTrees(TestASTNodeBase actual, TestASTNodeBase expected)
        {
            return expected.Equals(actual);
        }
    }
}