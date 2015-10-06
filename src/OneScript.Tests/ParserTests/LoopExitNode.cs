using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Tests
{
    class LoopExitNode : TestASTNodeBase, IASTNode
    {
        public LoopExitNode(bool isBreak)
        {
            IsBreak = isBreak;
        }

        public bool IsBreak { get; set; }

        protected override bool EqualsInternal(IASTNode other)
        {
            throw new NotImplementedException();
        }
    }
}
