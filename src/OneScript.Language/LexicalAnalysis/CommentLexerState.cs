/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    class CommentLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            iterator.MoveNext();
            System.Diagnostics.Debug.Assert(iterator.CurrentSymbol == '/');

            return new Lexem()
            {
                Content = iterator.ReadToLineEnd(),
                Type = LexemType.Comment,
                Token = Token.NotAToken
            };
        }
    }
}
