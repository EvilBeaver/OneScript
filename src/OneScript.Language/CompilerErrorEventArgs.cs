using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public class CompilerErrorEventArgs : EventArgs
    {
        public ScriptException Exception { get; set; }
        public ILexemGenerator LexemGenerator { get; set; }
        public bool IsHandled { get; set; }
    }
}
