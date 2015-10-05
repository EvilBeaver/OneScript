using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Tests
{
    class ForEachNode : TestASTNodeBase, IASTForEachNode
    {

        protected override bool EqualsInternal(IASTNode other)
        {
            throw new NotImplementedException();
        }

        public IASTNode CollectionExpression
        {
            get;
            set;
        }

        public IASTNode ItemIdentifier
        {
            get;
            set;
        }

        public IASTNode Body
        {
            get;
            set;
        }
    }
}
