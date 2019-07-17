/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.LexicalAnalysis
{
    public class NumberLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            bool hasDecimalPoint = false;
            while (true)
            {
                if (Char.IsDigit(iterator.CurrentSymbol))
                {
                    if (!iterator.MoveNext())
                    {
                        var lex = new Lexem()
                        {
                            Type = LexemType.NumberLiteral,
                            Content = iterator.GetContents()
                        };

                        return lex;
                    }
                }
                else if (SpecialChars.IsDelimiter(iterator.CurrentSymbol))
                {
                    if (iterator.CurrentSymbol == '.')
                    {
                        if (!hasDecimalPoint)
                        {
                            hasDecimalPoint = true;
                            iterator.MoveNext();
                            continue;
                        }
                        else
                        {
                            throw CreateExceptionOnCurrentLine("Некорректно указана десятичная точка в числе", iterator);
                        }
                    }

                    var lex = new Lexem()
                    {
                        Type = LexemType.NumberLiteral,
                        Content = iterator.GetContents()
                    };

                    return lex;
                }
                else
                {
                    throw CreateExceptionOnCurrentLine("Некорректный символ", iterator);
                }
            }
        }
    }
}
