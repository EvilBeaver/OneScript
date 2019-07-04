using System;
using System.Collections.Generic;
using System.Text;

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
