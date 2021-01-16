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
        
        public AstNodeAppendingHandler(IAstBuilder nodeBuilder, IErrorSink errorSink) : base(nodeBuilder, errorSink)
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

        protected override void ParseAnnotationInternal(ref Lexem lastExtractedLexem, ILexer lexer)
        {
            var node = NodeBuilder.CreateNode(NodeKind.Preprocessor, lastExtractedLexem);
            _allLineContentLexer.Iterator = lexer.Iterator;

            lastExtractedLexem = _allLineContentLexer.NextLexemOnSameLine();
            if (lastExtractedLexem.Type != LexemType.EndOfText)
            {
                var child = NodeBuilder.CreateNode(NodeKind.Unknown, lastExtractedLexem);
                NodeBuilder.AddChild(node, child);
            }

            lastExtractedLexem = lexer.NextLexem();
            NodeBuilder.AddChild(NodeBuilder.CurrentNode, node);
        }
    }
}