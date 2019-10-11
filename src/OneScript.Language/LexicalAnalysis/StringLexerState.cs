/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;

namespace OneScript.Language.LexicalAnalysis
{
    public class StringLexerState : LexerState
    {
        private void SkipSpacesAndComments(SourceCodeIterator iterator)
        {
            while (true)
            {   /* Пропускаем все пробелы и комментарии */
                iterator.SkipSpaces();

                if (iterator.CurrentSymbol == '/')
                {
                    if (!iterator.MoveNext())
                        throw CreateExceptionOnCurrentLine("Некорректный символ", iterator);

                    if (iterator.CurrentSymbol != '/')
                        throw CreateExceptionOnCurrentLine("Некорректный символ", iterator);

                    do
                    {
                        if (!iterator.MoveNext())
                            break;

                    } while (iterator.CurrentSymbol != '\n');

                }
                else
                    break;
            }
        }

        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            StringBuilder contentBuilder = new StringBuilder();

            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;

                if (cs == SpecialChars.StringQuote)
                {
                    if (iterator.MoveNext())
                    {
                        if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                        {
                            /* Двойная кавычка */
                            contentBuilder.Append("\"");
                            continue;
                        }

                        /* Завершение строки */
                        SkipSpacesAndComments(iterator);

                        if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                        {
                            /* Сразу же началась новая строка */
                            contentBuilder.Append('\n');
                            continue;
                        }
                    }

                    var lex = new Lexem
                    {
                        Type = LexemType.StringLiteral,
                        Content = contentBuilder.ToString()
                    };
                    return lex;
                }
                
                if (cs == '\n')
                {
                    iterator.MoveNext();
                    SkipSpacesAndComments(iterator);

                    if (iterator.CurrentSymbol != '|')
                        throw CreateExceptionOnCurrentLine("Некорректный строковый литерал!", iterator);

                    contentBuilder.Append('\n');
                }
                else if(cs != '\r')
                    contentBuilder.Append(cs);

            }

            throw CreateExceptionOnCurrentLine("Незавершённый строковой интервал!", iterator);
        }
    }
}
