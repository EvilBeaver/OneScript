using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class Lexer
    {
        private string _code;

        public Lexer()
        {
            Position = OUT_OF_TEXT;
            CurrentLine = OUT_OF_TEXT;
        }

        public int Position { get; private set; }
        public int CurrentLine { get; private set; }
        public string Code 
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                Position = 0;
                CurrentLine = 0;
            }
        }

        public Lexem NextLexem()
        {
            return Lexem.Empty();
        }

        public const int OUT_OF_TEXT = -1;
    }
}
