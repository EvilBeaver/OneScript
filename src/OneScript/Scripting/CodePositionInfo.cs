using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    class CodePositionInfo
    {
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Code { get; set; }
    }
}
