using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class CodePositionInfo
    {
        public CodePositionInfo()
        {
            LineNumber = -1;
            ColumnNumber = -1;
        }

        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Code { get; set; }
    }
}
