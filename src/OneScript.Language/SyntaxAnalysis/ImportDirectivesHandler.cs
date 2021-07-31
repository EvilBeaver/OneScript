/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ImportDirectivesHandler : ModuleAnnotationDirectiveHandler
    {
        private readonly ILexer _importClauseLexer;

        public ImportDirectivesHandler(IAstBuilder nodeBuilder, IErrorSink errorSink) : base(nodeBuilder, errorSink)
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            _importClauseLexer = builder.Build();
        }

        protected override void ParseAnnotationInternal(ref Lexem lastExtractedLexem, ILexer lexer)
        {
            var node = NodeBuilder.CreateNode(NodeKind.Import, lastExtractedLexem);
            _importClauseLexer.Iterator = lexer.Iterator;
            
            var lex = _importClauseLexer.NextLexem();
            if (lex.Type == LexemType.EndOfText)
            {
                ErrorSink.AddError(LocalizedErrors.LibraryNameExpected());
                return;
            }

            var argumentNode = NodeBuilder.CreateNode(NodeKind.AnnotationParameter, lex);
            var value = NodeBuilder.CreateNode(NodeKind.AnnotationParameterValue, lex);
            NodeBuilder.AddChild(argumentNode, value);
            
            NodeBuilder.AddChild(node, argumentNode);
            NodeBuilder.AddChild(NodeBuilder.ContextNode, node);

            lex = _importClauseLexer.NextLexemOnSameLine();
            if (lex.Type != LexemType.EndOfText)
            {
                ErrorSink.AddError(LocalizedErrors.UnexpectedOperation());
                return;
            }

            lastExtractedLexem = lexer.NextLexem();
        }
        
        protected override bool DirectiveSupported(string directive)
        {
            return LanguageDef.IsImportDirective(directive);
        }
    }
}