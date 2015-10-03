using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Tests
{
    class ForLoopNode : TestASTNodeBase, IASTForLoopNode
    {
        public IASTNode Body
        {
            get;
            set;
        }

        public IASTNode BoundExpression
        {
            get;
            set;
        }

        public IASTNode InitializerExpression
        {
            get;
            set;
        }

        public IASTNode LoopCounter { get; set; }

        protected override bool EqualsInternal(IASTNode other)
        {
            throw new NotImplementedException();
        }
    }
}
