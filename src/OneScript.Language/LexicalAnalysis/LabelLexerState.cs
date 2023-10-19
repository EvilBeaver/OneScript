using System.Diagnostics;
using OneScript.Localization;

namespace OneScript.Language.LexicalAnalysis
{
    public class LabelLexerState : LexerState
    {
        private static BilingualString MESSAGE_NAME_EXPECTED = new BilingualString(
            "Ожидается имя метки",
            "Label name expected"
        );

        WordLexerState _wordExtractor = new WordLexerState();
        
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            Debug.Assert(iterator.CurrentSymbol == SpecialChars.Tilde);

            var start = new CodeRange(iterator.CurrentLine, iterator.CurrentColumn);
            iterator.MoveNext();
            if (!iterator.MoveToContent())
                throw CreateExceptionOnCurrentLine(MESSAGE_NAME_EXPECTED.ToString(), iterator);

            if (!char.IsLetter(iterator.CurrentSymbol))
                throw CreateExceptionOnCurrentLine(MESSAGE_NAME_EXPECTED.ToString(), iterator);
            
            var result = _wordExtractor.ReadNextLexem(iterator);
            result.Type = LexemType.LabelRef;
            if (iterator.CurrentSymbol == SpecialChars.Colon)
            {
                result.Type = LexemType.Label;
                iterator.MoveNext();
            }

            result.Location = start;
            return result;
        }
    }
}