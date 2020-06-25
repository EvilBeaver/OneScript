/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class NonTerminalNode : AstNodeBase
    {
        private List<AstNodeBase> _children = new List<AstNodeBase>();
        
        public IEnumerable<AstNodeBase> Children => _children;

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