/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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
        
        private static BilingualString INVALID_LABEL = new BilingualString(
            "Неверно задана метка",
            "Invalid label definition"
        );

        private readonly WordLexerState _wordExtractor = new WordLexerState();
        
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
            if (!LanguageDef.IsUserSymbol(result))
            {
                throw CreateExceptionOnCurrentLine(INVALID_LABEL.ToString(), iterator);
            }
            
            result.Type = LexemType.LabelRef;
            if (iterator.CurrentSymbol == SpecialChars.Colon)
            {
                result.Type = LexemType.Label;
                var tail = iterator.ReadToLineEnd();
                if (tail.Trim().Length != 0)
                {
                    throw CreateExceptionOnCurrentLine(INVALID_LABEL.ToString(), iterator);
                }
            }

            result.Location = start;
            return result;
        }
    }
}