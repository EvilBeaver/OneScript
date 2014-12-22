using OneScript.Scripting.Compiler;

namespace OneScript.Tests
{
    class IndexedAccessNode : TestASTNodeBase
    {
        IASTNode _target;
        IASTNode _index;

        public IndexedAccessNode(IASTNode target, IASTNode index)
        {
            _target = target;
            _index = index;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherTest = (IndexedAccessNode)other;
            var target = (TestASTNodeBase)_target;
            var index = (TestASTNodeBase)_index;

            return target.Equals(otherTest._target) && index.Equals(otherTest._index);

        }
    }
}