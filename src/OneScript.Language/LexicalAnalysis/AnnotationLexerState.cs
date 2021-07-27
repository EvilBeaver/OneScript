/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public class AnnotationLexerState : LexerState
    {
        WordLexerState _wordExtractor = new WordLexerState();
        const string MESSAGE_ANNOTATION_EXPECTED = "Ожидается имя аннотации";

        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            iterator.MoveNext();

            if (!iterator.MoveToContent())
                throw CreateExceptionOnCurrentLine(MESSAGE_ANNOTATION_EXPECTED, iterator);

            if (!char.IsLetter(iterator.CurrentSymbol))
                throw CreateExceptionOnCurrentLine(MESSAGE_ANNOTATION_EXPECTED, iterator);

            var lexem = _wordExtractor.ReadNextLexem(iterator);
            lexem.Type = LexemType.Annotation;
            return lexem;
        }
    }
}