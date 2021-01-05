/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class PreprocessingLexer : FullSourceLexer
    {
        public PreprocessorHandlers Handlers { get; set; }
        
        public override Lexem NextLexem()
        {
            Lexem lex;
            while ((lex = base.NextLexem()).Type == LexemType.PreprocessorDirective)
            {
                var fakeContext = new ParserContext(this, default, lex);
                var handled = Handlers.HandleDirective(fakeContext);
                if (handled)
                {
                    return fakeContext.LastExtractedLexem;
                }

                throw new SyntaxErrorException(this.GetErrorPosition(), 
                    LocalizedErrors.DirectiveNotSupported(lex.Content));
            }

            return lex;
        }
    }
}