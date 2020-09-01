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
    public class TerminalNode : BslSyntaxNode
    {
        public Lexem Lexem { get; }
        
        public TerminalNode(int kind)
        {
            Kind = kind;
        }
        
        public TerminalNode(int kind, Lexem lexem)
        {
            Kind = kind;
            Lexem = lexem;
            Location = lexem.Location;
        }

        public override IReadOnlyList<BslSyntaxNode> Children => EmptyChildren;
        
        private static readonly BslSyntaxNode[] EmptyChildren = new BslSyntaxNode[0];
    }
}