/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public class AnnotationLexerState : WordLexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            var lexem = base.ReadNextLexem(iterator);
            lexem.Type = LexemType.Annotation;
            return lexem;
        }
    }
}