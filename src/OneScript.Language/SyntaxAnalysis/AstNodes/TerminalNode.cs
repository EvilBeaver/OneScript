/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class TerminalNode : AstNodeBase
    {
        public Lexem Lexem { get; set; }
        
        public TerminalNode(int kind)
        {
            Kind = kind;
        }
        
        public TerminalNode(int kind, Lexem lexem)
        {
            Kind = kind;
            Lexem = lexem;
        }
    }
}