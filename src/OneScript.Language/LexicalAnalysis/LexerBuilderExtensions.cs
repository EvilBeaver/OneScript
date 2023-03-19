/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public static class LexerBuilderExtensions
    {
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

        public static LexerBuilder DetectSemicolons(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == SpecialChars.EndOperator)
                .HandleWith(new SpecialLexerState(LexemType.EndOperator, Token.Semicolon));

            return builder;
        }
        
        public static LexerBuilder DetectTernaryOperators(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == SpecialChars.QuestionMark)
                .HandleWith(new SpecialLexerState(LexemType.Operator, Token.Question));

            return builder;
        }
        
        public static LexerBuilder DetectAnnotations(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == SpecialChars.Annotation)
                .HandleWith(new AnnotationLexerState());

            return builder;
        }
        
        public static LexerBuilder DetectDates(this LexerBuilder builder)
        {
            builder.Detect((cs, i) => cs == SpecialChars.DateQuote)
                .HandleWith(new DateLexerState());

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

    public class SpecialLexerState : LexerState
    {
        private readonly LexemType _type;
        private readonly Token _token;

        public SpecialLexerState(LexemType type, Token token)
        {
            _type = type;
            _token = token;
        }

        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            iterator.MoveNext();
            return new Lexem
            {
                Type = _type,
                Token = _token,
                Location = new CodeRange(iterator.CurrentLine, iterator.CurrentColumn)
            };
        }
    }
}