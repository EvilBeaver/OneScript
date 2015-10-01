using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    class CommentLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            iterator.MoveNext();
            System.Diagnostics.Debug.Assert(iterator.CurrentSymbol == '/');

            while (iterator.MoveNext())
            {
                if (iterator.CurrentSymbol == '\n')
                {
                    return CreateCommentLexem(iterator);
                }
            }

            return CreateCommentLexem(iterator);

        }

        private Lexem CreateCommentLexem(SourceCodeIterator iterator)
        {
            return new Lexem()
            {
                Content = iterator.GetContents().TrimEnd('\r', '\n'),
                Type = LexemType.Comment,
                Token = Token.NotAToken
            };
        }

    }
}
