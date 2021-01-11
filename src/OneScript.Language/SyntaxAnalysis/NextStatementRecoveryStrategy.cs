/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using System.Linq;

namespace OneScript.Language.SyntaxAnalysis
{
    public class NextStatementRecoveryStrategy : IErrorRecoveryStrategy
    {
        public Lexem Recover(ILexer lexer)
        {
            if(!lexer.Iterator.MoveToContent())
                return Lexem.EndOfText();

            Lexem lastLexem = default;
            do
            {
                try
                {
                    lastLexem = lexer.NextLexem();
                    if(AdditionalStops != null && AdditionalStops.Contains(lastLexem.Token) )
                    {
                        break;
                    }
                }
                catch (SyntaxErrorException)
                {
                    lexer.Iterator.MoveNext();
                }
                
            } while (!(lastLexem.Token == Token.EndOfText
                       || LanguageDef.IsBeginOfStatement(lastLexem.Token)));

            return lastLexem;
        }

        public Token[] AdditionalStops { get; set; }
    }
}