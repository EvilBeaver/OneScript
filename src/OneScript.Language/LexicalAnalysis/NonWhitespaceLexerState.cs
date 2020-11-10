// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public class NonWhitespaceLexerState : LexerState
    {
        public bool DiscardComments { get; set; } = true;

        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            var currentLine = iterator.CurrentLine;
            var currentColumn = iterator.CurrentColumn;
            string content;
            if (iterator.CurrentSymbol == '"')
                content = QuotedValue(iterator);
            else
                content = UnquotedValue(iterator);
            
            return new Lexem
            {
                Content = content,
                Location = new CodeRange(currentLine, currentColumn),
                Type = LexemType.NotALexem,
                Token = Token.NotAToken
            };
        }

        private string QuotedValue(SourceCodeIterator iterator)
        {
            while (iterator.MoveNext() && iterator.CurrentSymbol != '"')
            {
                if(iterator.CurrentSymbol == '\n')
                    throw CreateExceptionOnCurrentLine("Незавершенный строковый литерал", iterator);
            }

            if(iterator.CurrentSymbol != '"')
                throw CreateExceptionOnCurrentLine("Незавершенный строковый литерал", iterator);

            iterator.MoveNext();
            return iterator.GetContents();
        }

        private string UnquotedValue(SourceCodeIterator iterator)
        {
            string content = default;
            while (!char.IsWhiteSpace(iterator.CurrentSymbol))
            {
                if (DiscardComments && iterator.CurrentSymbol == '/' && iterator.PeekNext() == '/')
                {
                    content = iterator.GetContents();
                    SkipToLineEnd(iterator);
                    break;
                }

                if (!iterator.MoveNext())
                {
                    content = iterator.GetContents();
                    break;
                }
            }

            return content ?? iterator.GetContents();
        }

        private void SkipToLineEnd(SourceCodeIterator iterator)
        {
            while (iterator.MoveNext() && iterator.CurrentSymbol != '\n')
            {
            }

            iterator.GetContentSpan();
        }
    }
}