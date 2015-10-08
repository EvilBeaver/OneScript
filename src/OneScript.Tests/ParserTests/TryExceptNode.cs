using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Tests
{
    class TryExceptNode : TestASTNodeBase, IASTTryExceptNode
    {
        private CodeBatchNode _tryBody;
        private CodeBatchNode _exceptBody;
        
        protected override bool EqualsInternal(IASTNode other)
        {
            throw new NotImplementedException();
        }

        public IASTNode TryBlock
        {
            get
            {
                return _tryBody;
            }
            set
            {
                _tryBody = (CodeBatchNode)value;
            }
        }

        public IASTNode ExceptBlock
        {
            get
            {
                return _exceptBody;
            }
            set
            {
                _exceptBody = (CodeBatchNode)value;
            }
        }
    }
}
