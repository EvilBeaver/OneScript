using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public class Lexer : FullSourceLexer
    {
        public override Lexem NextLexem()
        {
            Lexem lex;
            while((lex = base.NextLexem()).Type == LexemType.Comment)
                ; // skip

            return lex;
        }
    }
}
