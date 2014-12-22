using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler.Lexics
{
    public class PreprocessorDirectiveLexerState : LexerState
    {
        WordLexerState _wordExtractor = new WordLexerState();

        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            System.Diagnostics.Debug.Assert(iterator.CurrentSymbol == SpecialChars.Preprocessor);

            iterator.MoveNext();

            if (!Char.IsLetter(iterator.CurrentSymbol))
                CreateExceptionOnCurrentLine("Ожидается директива препроцессора", iterator);
            
            iterator.MoveToContent();

            var lex = _wordExtractor.ReadNextLexem(iterator);

            lex.Type = LexemType.PreprocessorDirective;

            return lex;

        }
    }
}
