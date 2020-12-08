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

        public ImportDirectivesHandler()
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            _importClauseLexer = builder.Build();
        }
        
        protected override bool HandleDirectiveInternal(ParserContext context)
        {
            var lastExtractedLexem = context.LastExtractedLexem;
            var lexemStream = context.Lexer;
            var nodeBuilder = context.NodeBuilder;
            if (!DirectiveSupported(lastExtractedLexem.Content))
            {
                return default;
            }
            
            var node = nodeBuilder.CreateNode(NodeKind.Import, lastExtractedLexem);
            _importClauseLexer.Iterator = lexemStream.Iterator;
            
            var lex = _importClauseLexer.NextLexem();
            if (lex.Type == LexemType.EndOfText)
            {
                throw new SyntaxErrorException(lexemStream.GetErrorPosition(),
                    "Ожидается имя библиотеки");
            }

            var argumentNode = nodeBuilder.CreateNode(NodeKind.AnnotationParameter, lex);
            var value = nodeBuilder.CreateNode(NodeKind.AnnotationParameterValue, lex);
            nodeBuilder.AddChild(argumentNode, value);
            
            nodeBuilder.AddChild(node, argumentNode);
            nodeBuilder.AddChild(context.NodeContext.Peek(), node);

            lex = _importClauseLexer.NextLexemOnSameLine();
            if (lex.Type != LexemType.EndOfText)
            {
                throw new SyntaxErrorException(lexemStream.GetErrorPosition(),
                    LocalizedErrors.UnexpectedOperation());
            }

            context.LastExtractedLexem = lexemStream.NextLexem(); 

            return true;
        }
        
        private static bool DirectiveSupported(string directive)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(directive, "использовать") == 0
                   || StringComparer.InvariantCultureIgnoreCase.Compare(directive, "use") == 0;
        }
    }
}