/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public abstract class BranchingStatementNode : NonTerminalNode
    {
        protected BranchingStatementNode(int kind) : base(kind)
        {
        }
        
        protected BranchingStatementNode(int kind, Lexem startLexem) : base(kind, startLexem)
        {
        }


        public CodeRange EndLocation { get; private set; }
        
        protected override void OnChildAdded(BslSyntaxNode child)
        {
            EndLocation = child.Location;
        }
    }
}