using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler
{
    public class CompilerErrorEventArgs : EventArgs
    {
        public ScriptException Exception { get; set; }
        public Lexics.Lexer LexerState { get; set; }
        public bool IsHandled { get; set; }
    }
}
