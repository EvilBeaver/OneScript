/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class CallNode : NonTerminalNode
    {
        public CallNode(int kind, Lexem startLexem) : base (kind, startLexem)
        {
        }
        
        public TerminalNode Identifier { get; private set; }
        
        public BslSyntaxNode ArgumentList { get; private set; }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            if (child.Kind == NodeKind.Identifier)
            {
                Identifier = (TerminalNode) child;
            }
            else
            {
                ArgumentList = (NonTerminalNode)child;
            }
        }
    }
}