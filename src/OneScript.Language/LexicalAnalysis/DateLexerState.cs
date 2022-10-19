/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;

namespace OneScript.Language.LexicalAnalysis
{
    public class DateLexerState : LexerState
    {
        private const string fullZeroes = "00000000000000";
        private const string minutesZeroes = "000000000000";
        private const string dateZeroes = "00000000";
        
        private string FullDateTimeString( StringBuilder numbers )
        {
            string formatString = "yyyyMMddHHmmss";
            string compare = fullZeroes;
            if (numbers.Length == 12) // yyyyMMddHHmm
            {
                formatString = "yyyyMMddHHmm";
                compare = minutesZeroes;
            }
            else if (numbers.Length == 8) // yyyyMMdd
            {
                formatString = "yyyyMMdd";
                compare = dateZeroes;
            }
            else if (numbers.Length != 14) // yyyyMMddHHmmss
            {
                throw new FormatException();
            }
            
            string date = numbers.ToString();

            if (date != compare)
                DateTime.ParseExact(date, formatString, System.Globalization.CultureInfo.InvariantCulture);

            return date;
        }
        
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            var numbers = new StringBuilder();

            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;
                if (cs == SpecialChars.DateQuote)
                {
                    iterator.GetContentSpan();
                    iterator.MoveNext();

                    try
                    {
                        var lex = new Lexem()
                        {
                            Type = LexemType.DateLiteral,
                            Content = FullDateTimeString(numbers),
                            Location = new CodeRange(iterator.CurrentLine, iterator.CurrentColumn)
                        };

                        return lex;
                    }
                    catch( FormatException )
                    {
                        throw CreateExceptionOnCurrentLine("Некорректный литерал даты", iterator);
                    }
                }

                if (Char.IsDigit(cs))
                {
                    numbers.Append(cs);
                }

                if (numbers.Length > 14)
                    throw CreateExceptionOnCurrentLine("Некорректный литерал даты", iterator);
            }

            throw CreateExceptionOnCurrentLine("Незавершенный литерал даты", iterator);
        }
    }
}
