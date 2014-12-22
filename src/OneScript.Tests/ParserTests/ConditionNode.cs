using OneScript.Scripting.Compiler;

namespace OneScript.Tests
{
    class ConditionNode : TestASTNodeBase, IASTIfNode
    {
        private IASTNode _condition;

        CodeBatchNode _truePart;
        CodeBatchNode _falsePart;

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherConditionNode = other as ConditionNode;
            var expression = (TestASTNodeBase)(otherConditionNode._condition);
            return (expression.Equals(_condition)
            && _truePart.Equals(otherConditionNode._truePart)
            && _falsePart.Equals(otherConditionNode._falsePart));
        }

        public IASTNode Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = value;
            }
        }

        public IASTNode TruePart
        {
            get
            {
                return _truePart;
            }
            set
            {
                _truePart = (CodeBatchNode)value;
            }
        }

        public IASTNode FalsePart
        {
            get
            {
                return _falsePart;
            }
            set
            {
                _falsePart = (CodeBatchNode)value;
            }
        }
    }
}