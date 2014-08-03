using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
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
