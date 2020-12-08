/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.Tests
{
    public class AstNodeAppendingHandler : IDirectiveHandler
    {
        private readonly ILexer _allLineContentLexer;
        
        public AstNodeAppendingHandler()
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new FullLineLexerState());

            _allLineContentLexer = builder.Build();
        }

        public void OnModuleEnter(ParserContext context)
        {
        }

        public void OnModuleLeave(ParserContext context)
        {
        }

        public bool HandleDirective(ParserContext context)
        {
            var lastExtractedLexem = context.LastExtractedLexem;
            var lexemStream = context.Lexer;
            var node = context.NodeBuilder.CreateNode(NodeKind.Preprocessor, lastExtractedLexem);
            _allLineContentLexer.Iterator = lexemStream.Iterator;

            lastExtractedLexem = _allLineContentLexer.NextLexemOnSameLine();
            if (lastExtractedLexem.Type != LexemType.EndOfText)
            {
                var child = context.NodeBuilder.CreateNode(NodeKind.Unknown, lastExtractedLexem);
                context.NodeBuilder.AddChild(node, child);
            }

            context.LastExtractedLexem = lexemStream.NextLexem();
            context.NodeBuilder.AddChild(context.NodeContext.Peek(), node);
            return true;
        }
    }
}