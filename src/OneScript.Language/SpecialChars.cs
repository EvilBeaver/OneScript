using System;
using System.Collections.Generic;
using System.Text;
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language
{
    public static class SpecialChars
    {
        public const char StringQuote = '"';
        public const char DateQuote = '\'';
        public const char EndOperator = ';';
        public const char Underscore = '_';
        public const char QuestionMark = '?';
        public const char Preprocessor = '#';
        public const char Annotation = '&';

        public static bool IsOperatorChar(char symbol)
        {
            switch (symbol)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '<':
                case '>':
                case '=':
                case '%':
                case '(':
                case ')':
                case '.':
                case ',':
                case '[':
                case ']':
                    return true;
                default:
                    return false;

            }
        }

        public static bool IsDelimiter(char symbol)
        {
            return !(Char.IsLetterOrDigit(symbol) || symbol == Underscore);
        }

    }
}
