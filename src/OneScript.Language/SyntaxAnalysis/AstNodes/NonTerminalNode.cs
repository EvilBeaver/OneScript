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
    public class NonTerminalNode : AstNodeBase
    {
        private List<AstNodeBase> _children = new List<AstNodeBase>();

        public NonTerminalNode(NodeKind kind, Lexem startLexem)
            :this(kind)
        {
            Location = startLexem.Location;
        }
        
        protected NonTerminalNode()
        {
        }
        
        public NonTerminalNode(NodeKind kind)
        {
            Kind = kind;
        }

        public IReadOnlyList<AstNodeBase> Children => _children;

        public void AddChild(AstNodeBase child)
        {
            child.Parent = this;
            _children.Add(child);
            OnChildAdded(child);
        }

        protected virtual void OnChildAdded(AstNodeBase child)
        {
        }
    }
}