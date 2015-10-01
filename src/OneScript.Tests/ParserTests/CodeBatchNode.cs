using OneScript.Language;
using System.Collections.Generic;

namespace OneScript.Tests
{
    class CodeBatchNode : TestASTNodeBase
    {
        List<TestASTNodeBase> _children = new List<TestASTNodeBase>();

        public List<TestASTNodeBase> Children
        {
            get { return _children; }
        }

        public void Add(IASTNode node)
        {
            var casted = (TestASTNodeBase)node;
            _children.Add(casted);

        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var casted = (CodeBatchNode)other;

            if (casted._children.Count != _children.Count)
                return false;

            bool allMatch = true;

            for (int i = 0; i < _children.Count; i++)
            {
                allMatch = allMatch && _children[i].Equals(casted._children[i]);
                if (!allMatch)
                    break;
            }

            return allMatch;

        }
    }
}