/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class NonTerminalNode : BslSyntaxNode
    {
        private List<BslSyntaxNode> _children = new List<BslSyntaxNode>();

        public NonTerminalNode(int kind, Lexem startLexem)
            :this(kind)
        {
            Location = startLexem.Location;
        }
        
        protected NonTerminalNode()
        {
        }
        
        public NonTerminalNode(int kind)
        {
            Kind = kind;
        }

        public override IReadOnlyList<BslSyntaxNode> Children => _children;

        public void AddChild(BslSyntaxNode child)
        {
            child.Parent = this;
            _children.Add(child);
            OnChildAdded(child);
        }

        protected virtual void OnChildAdded(BslSyntaxNode child)
        {
        }
    }
}