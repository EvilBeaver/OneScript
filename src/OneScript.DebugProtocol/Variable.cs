using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.DebugProtocol
{
    public class Variable
    {
        public string Name { get; set; }

        public bool IsStructured { get; set; }

        public string Presentation { get; set; }

        public string TypeName { get; set; }
    }
}
