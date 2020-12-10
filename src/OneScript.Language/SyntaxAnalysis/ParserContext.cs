/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ParserContext
    {
        public ILexer Lexer { get; }
        
        public Stack<BslSyntaxNode> NodeContext { get; }
        
        public IAstBuilder NodeBuilder { get; }
        
        public Lexem LastExtractedLexem { get; set; }

        public ParserContext(ILexer lexer, Stack<BslSyntaxNode> nodeContext, IAstBuilder builder, Lexem lastLexem)
        {
            Lexer = lexer;
            NodeContext = nodeContext;
            NodeBuilder = builder;
            LastExtractedLexem = lastLexem;
        }
    }
}