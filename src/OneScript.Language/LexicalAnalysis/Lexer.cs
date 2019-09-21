/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public class Lexer : FullSourceLexer
    {
        public override Lexem NextLexem()
        {
            Lexem lex;
            while((lex = base.NextLexem()).Type == LexemType.Comment)
                ; // skip

            return lex;
        }
    }
}