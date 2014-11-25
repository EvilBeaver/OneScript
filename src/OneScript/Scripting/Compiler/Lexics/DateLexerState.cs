using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler.Lexics
{
    public class DateLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;
                if (cs == SpecialChars.DateQuote)
                {
                    var lex = new Lexem()
                    {
                        Type = LexemType.DateLiteral,
                        Content = iterator.GetContents(1, 1)
                    };

                    iterator.MoveNext();

                    return lex;
                }
                else if(!Char.IsDigit(cs))
                {
                    throw CreateExceptionOnCurrentLine("Незавершенный литерал даты", iterator);
                }
            }

            throw CreateExceptionOnCurrentLine("Незавершенный литерал даты", iterator);
        }
    }
}
