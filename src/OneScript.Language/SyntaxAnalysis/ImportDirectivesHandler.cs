/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
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

        protected override void ParseAnnotationInternal(string directiveName, ParserContext context)
        {
            var lexemStream = context.Lexer;
            var nodeBuilder = context.NodeBuilder;
            
            var node = nodeBuilder.CreateNode(NodeKind.Import, context.LastExtractedLexem);
            _importClauseLexer.Iterator = lexemStream.Iterator;
            
            var lex = _importClauseLexer.NextLexem();
            if (lex.Type == LexemType.EndOfText)
            {
                ErrorSink.AddError(LocalizedErrors.LibraryNameExpected());
                return;
            }

            var argumentNode = nodeBuilder.CreateNode(NodeKind.AnnotationParameter, lex);
            var value = nodeBuilder.CreateNode(NodeKind.AnnotationParameterValue, lex);
            nodeBuilder.AddChild(argumentNode, value);
            
            nodeBuilder.AddChild(node, argumentNode);
            nodeBuilder.AddChild(context.NodeContext.Peek(), node);

            lex = _importClauseLexer.NextLexemOnSameLine();
            if (lex.Type != LexemType.EndOfText)
            {
                ErrorSink.AddError(LocalizedErrors.UnexpectedOperation());
                return;
            }

            context.NextLexem();
        }
        
        protected override bool DirectiveSupported(string directive)
        {
            return LanguageDef.IsImportDirective(directive);
        }
    }
}