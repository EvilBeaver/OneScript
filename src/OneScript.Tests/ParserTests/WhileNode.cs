using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Tests
{
    class WhileNode : TestASTNodeBase, IASTWhileNode
    {
        private TestASTNodeBase _condition;
        private TestASTNodeBase _body;

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherNode = other as WhileNode;

            return _condition.Equals(otherNode._condition) && _body.Equals(otherNode._body);
        }

        public IASTNode Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = (TestASTNodeBase)value;
            }
        }

        public IASTNode Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = (TestASTNodeBase)value;
            }
        }
    }
}
