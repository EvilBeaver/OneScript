using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.DebugProtocol
{
    public class StackFrame
    {
        public int Index { get; set; }

        public string MethodName { get; set; }

        public int LineNumber { get; set; }

        public string Source { get; set; }

        public Variable[] Variables { get; set; }
    }
}
