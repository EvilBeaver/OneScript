/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ParserContext
    {
        private readonly Stack<NonTerminalNode> _nodeContext = new Stack<NonTerminalNode>();
        
        public void PushContext(NonTerminalNode node)
        {
            _nodeContext.Push(node);
        }

        public NonTerminalNode PopContext()
        {
            return _nodeContext.Pop();
        }

        public NonTerminalNode CurrentParent => _nodeContext.Peek();

        public T AddChild<T>(T child) where T : BslSyntaxNode
        {
            CurrentParent.AddChild(child);
            return child;
        }
    }
}