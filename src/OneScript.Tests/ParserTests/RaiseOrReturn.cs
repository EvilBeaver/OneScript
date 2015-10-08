using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Tests
{
    class RaiseOrReturn : TestASTNodeBase, IASTNode
    {
        public IASTNode Expression { get; set; }
        public bool IsReturn { get; set; }
        
        protected override bool EqualsInternal(IASTNode other)
        {
            throw new NotImplementedException();
        }
    }
}
