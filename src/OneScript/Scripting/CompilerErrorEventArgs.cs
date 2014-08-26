using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class CompilerErrorEventArgs : EventArgs
    {
        public ScriptException Exception { get; set; }
        public Lexer LexerState { get; set; }
        public bool IsHandled { get; set; }
    }
}
