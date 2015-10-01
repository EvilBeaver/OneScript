using OneScript.Language;

namespace OneScript.Tests
{
    class PropertyAccessNode : TestASTNodeBase
    {
        IASTNode _target;
        string _name;

        public PropertyAccessNode(IASTNode target, string name)
        {
            _target = target;
            _name = name;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherTest = (PropertyAccessNode)other;
            var target = (TestASTNodeBase)_target;

            return target.Equals(otherTest._target) && _name.Equals(otherTest._name);

        }
    }
}