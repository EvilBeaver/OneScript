using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class LexemExtractor
    {
        private Lexer _lexer;
        private Lexem _lastExtractedLexem;

        public LexemExtractor(Lexer lexer)
        {
            _lexer = lexer;
        }

        public void Next()
        {
            if (_lastExtractedLexem.Token != Token.EndOfText)
            {
                _lastExtractedLexem = _lexer.NextLexem();
            }
            else
            {
                throw CompilerException.UnexpectedEndOfText();
            }
        }

        public Lexem LastExtractedLexem
        {
            get { return _lastExtractedLexem; }
        }

        public Lexer GetLexer()
        {
            return _lexer;
        }
    }
}
