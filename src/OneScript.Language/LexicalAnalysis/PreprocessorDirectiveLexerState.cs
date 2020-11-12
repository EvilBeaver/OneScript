/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.LexicalAnalysis
{
    public class PreprocessorDirectiveLexerState : LexerState
    {
        WordLexerState _wordExtractor = new WordLexerState();
        const string MESSAGE_DIRECTIVE_EXPECTED = "Ожидается директива препроцессора";

        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            System.Diagnostics.Debug.Assert(iterator.CurrentSymbol == SpecialChars.Preprocessor);

            if (!iterator.OnNewLine)
                throw CreateExceptionOnCurrentLine("Недопустимое начало директивы препроцессора", iterator);

            iterator.MoveNext();
            var position = iterator.GetPositionInfo();
            if (!iterator.MoveToContent())
                throw CreateExceptionOnCurrentLine(MESSAGE_DIRECTIVE_EXPECTED, iterator);
            if (position.LineNumber != iterator.CurrentLine)
                throw new SyntaxErrorException(position, MESSAGE_DIRECTIVE_EXPECTED);

            if (!Char.IsLetter(iterator.CurrentSymbol))
                throw CreateExceptionOnCurrentLine(MESSAGE_DIRECTIVE_EXPECTED, iterator);
            
            var lex = _wordExtractor.ReadNextLexem(iterator);

            lex.Type = LexemType.PreprocessorDirective;

            return lex;
        }
    }
}
