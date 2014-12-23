using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine
{
    class CodePositionInfo
    {
        public string ModuleName { get; set; }
        public int LineNumber { get; set; }
        public string Code { get; set; }
    }
}
