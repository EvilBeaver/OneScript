using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler.Lexics
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
            StringBuilder ContentBuilder = new StringBuilder();

            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;

                if (cs == SpecialChars.StringQuote)
                {
                    iterator.MoveNext();
                    if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                    { /* Двойная кавычка */
                        ContentBuilder.Append("\"");
                        continue;
                    }

                    /* Завершение строки */
                    SkipSpacesAndComments(iterator);

                    if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                    {   /* Сразу же началась новая строка */
                        ContentBuilder.Append('\n');
                        continue;
                    }

                    var lex = new Lexem
                    {
                        Type = LexemType.StringLiteral,
                        Content = ContentBuilder.ToString()
                    };
                    return lex;
                }
                else if (cs == '\n')
                {
                    iterator.MoveNext();
                    SkipSpacesAndComments(iterator);

                    if (iterator.CurrentSymbol != '|')
                        throw CreateExceptionOnCurrentLine("Некорректный строковый литерал!", iterator);

                    ContentBuilder.Append('\n');

                    continue;
                }
                else
                    ContentBuilder.Append(cs);

            }

            throw CreateExceptionOnCurrentLine("Незавершённый строковой интервал!", iterator);
        }
    }
}
