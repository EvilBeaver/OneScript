/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ImportDirectivesHandler : ModuleAnnotationDirectiveHandler
    {
        private readonly ILexer _importClauseLexer;

        public ImportDirectivesHandler(IErrorSink errorSink) : base(errorSink)
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            _importClauseLexer = builder.Build();
        }

        protected override void ParseAnnotationInternal(
            ref Lexem lastExtractedLexem, 
            ILexer lexer,
            ParserContext parserContext)
        {
            var node = new AnnotationNode(NodeKind.Import, lastExtractedLexem);
            _importClauseLexer.Iterator = lexer.Iterator;
            
            var lex = _importClauseLexer.NextLexem();
            if (lex.Type == LexemType.EndOfText)
            {
                ErrorSink.AddError(LocalizedErrors.LibraryNameExpected());
                return;
            }

            var argumentNode = new AnnotationParameterNode();
            var value = new TerminalNode(NodeKind.AnnotationParameterValue, lex);
            argumentNode.AddChild(value);
            
            node.AddChild(argumentNode);
            parserContext.AddChild(node);

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