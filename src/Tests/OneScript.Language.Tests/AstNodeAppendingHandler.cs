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
    public class AstNodeAppendingHandler : ModuleAnnotationDirectiveHandler
    {
        private readonly ILexer _allLineContentLexer;
        
        public AstNodeAppendingHandler(IErrorSink errorSink) : base(errorSink)
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new FullLineLexerState());

            _allLineContentLexer = builder.Build();
        }

        protected override bool DirectiveSupported(string directive)
        {
            return true;
        }

        protected override void ParseAnnotationInternal(
            ref Lexem lastExtractedLexem, 
            ILexer lexer,
            ParserContext parserContext)
        {
            var node = new PreprocessorDirectiveNode(lastExtractedLexem);
            _allLineContentLexer.Iterator = lexer.Iterator;

            lastExtractedLexem = _allLineContentLexer.NextLexemOnSameLine();
            if (lastExtractedLexem.Type != LexemType.EndOfText)
            {
                var child = new TerminalNode(NodeKind.Unknown, lastExtractedLexem);
                node.AddChild(child);
            }

            lastExtractedLexem = lexer.NextLexem();
            parserContext.AddChild(node);
        }
    }
}