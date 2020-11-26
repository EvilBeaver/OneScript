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
        private readonly IAstBuilder _nodeBuilder;
        private readonly ILexer _allLineContentLexer;
        
        public AstNodeAppendingHandler(IAstBuilder nodeBuilder)
        {
            _nodeBuilder = nodeBuilder;
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new FullLineLexerState());

            _allLineContentLexer = builder.Build();
        }

        public void OnModuleEnter(IAstBuilder nodeBuilder, ILexer lexemStream)
        {
        }

        public void OnModuleLeave(ILexer lexemStream)
        {
        }

        public BslSyntaxNode HandleDirective(BslSyntaxNode parent, ILexer lexemStream, ref Lexem lastExtractedLexem)
        {
            var node = _nodeBuilder.CreateNode(NodeKind.Preprocessor, lastExtractedLexem);
            _allLineContentLexer.Iterator = lexemStream.Iterator;

            lastExtractedLexem = _allLineContentLexer.NextLexemOnSameLine();
            if (lastExtractedLexem.Type != LexemType.EndOfText)
            {
                var child = _nodeBuilder.CreateNode(NodeKind.Unknown, lastExtractedLexem);
                _nodeBuilder.AddChild(node, child);
            }

            lastExtractedLexem = lexemStream.NextLexem();
            _nodeBuilder.AddChild(parent, node);
            return parent;
        }
    }
}