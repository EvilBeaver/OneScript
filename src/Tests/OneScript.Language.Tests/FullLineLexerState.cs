/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.Tests
{
    public class FullLineLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            if (!iterator.MoveToContent())
                return Lexem.EndOfText();
            
            var location = new CodeRange(iterator.CurrentLine, iterator.CurrentColumn);
            var content = iterator.ReadToLineEnd();
            return new Lexem
            {
                Content = content,
                Location = location,
                Type = LexemType.NotALexem
            };
        }
    }
}