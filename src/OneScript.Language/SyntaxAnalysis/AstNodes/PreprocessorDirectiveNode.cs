/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class PreprocessorDirectiveNode : NonTerminalNode
    {
        public PreprocessorDirectiveNode(Lexem startLexem) 
            : base (NodeKind.Preprocessor, startLexem)
        {
            DirectiveName = startLexem.Content;
        }
        
        public string DirectiveName { get; }
        
        public string DirectiveContents { get; private set; }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            if (child is TerminalNode t)
            {
                DirectiveContents = t.Lexem.Content;
            }
        }
    }
}