/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ImportDirectivesHandler : IDirectiveHandler
    {
        private readonly IAstBuilder _nodeBuilder;
        private readonly ILexer _importClauseLexer;

        public ImportDirectivesHandler(IAstBuilder nodeBuilder)
        {
            _nodeBuilder = nodeBuilder;
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            _importClauseLexer = builder.Build();
        }
        
        public void OnModuleEnter(ILexer lexemStream)
        {
        }

        public void OnModuleLeave(ILexer lexemStream)
        {
        }

        public BslSyntaxNode HandleDirective(BslSyntaxNode parent, ILexer lexemStream, ref Lexem lastExtractedLexem)
        {
            if (!DirectiveSupported(lastExtractedLexem.Content))
            {
                return default;
            }
            
            var node = _nodeBuilder.CreateNode(NodeKind.Preprocessor, lastExtractedLexem);
            _importClauseLexer.Iterator = lexemStream.Iterator;
            
            var lex = _importClauseLexer.NextLexem();
            if (lex.Type == LexemType.EndOfText)
            {
                throw new SyntaxErrorException(lexemStream.GetErrorPosition(),
                    "Ожидается имя библиотеки");
            }

            var argumentNode = _nodeBuilder.CreateNode(NodeKind.Unknown, lex);
            _nodeBuilder.AddChild(node, argumentNode);
            _nodeBuilder.AddChild(parent, node);

            lex = _importClauseLexer.NextLexemOnSameLine();
            if (lex.Type != LexemType.EndOfText)
            {
                throw new SyntaxErrorException(lexemStream.GetErrorPosition(),
                    LocalizedErrors.UnexpectedOperation());
            }

            lastExtractedLexem = lexemStream.NextLexem(); 

            return parent;
        }
        
        private bool DirectiveSupported(string directive)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(directive, "использовать") == 0
                   || StringComparer.InvariantCultureIgnoreCase.Compare(directive, "use") == 0;
        }
    }
}