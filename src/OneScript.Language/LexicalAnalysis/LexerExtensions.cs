/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public static class LexerExtensions
    {
        public static int ReadToLineEnd(this ILexer lexer)
        {
            var data = lexer.Iterator.ReadToLineEnd();
            return data.Length;
        }

        public static Lexem NextLexemOnSameLine(this ILexer lexer)
        {
            var currentLine = lexer.Iterator.CurrentLine;
            if (lexer.Iterator.MoveToContent())
            {
                if (currentLine == lexer.Iterator.CurrentLine)
                    return lexer.NextLexem();
            }

            return Lexem.EndOfText();
        }

        public static LexerBuilder DetectWords(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => char.IsLetter(cs) || cs == SpecialChars.Underscore)
                .HandleWith(new WordLexerState());
            return builder;
        }
        
        public static LexerBuilder DetectStrings(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == SpecialChars.StringQuote)
                .HandleWith(new StringLexerState());
            return builder;
        }
        
        public static LexerBuilder DetectOperators(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => SpecialChars.IsOperatorChar(cs) &&
                                      !(cs == '/' && i.PeekNext() == '/'))
                .HandleWith(new OperatorLexerState());
            return builder;
        }
        
        public static LexerBuilder DetectComments(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == '/' && i.PeekNext() == '/')
                .HandleWith(new CommentLexerState());
            return builder;
        }
        
        public static LexerBuilder DetectNumbers(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => char.IsDigit(cs))
                .HandleWith(new NumberLexerState());
            return builder;
        }
        
        public static LexerBuilder DetectPreprocessorDirectives(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == SpecialChars.Preprocessor)
                .HandleWith(new PreprocessorDirectiveLexerState());
            return builder;
        }
    }
}