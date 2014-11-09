using ScriptEngine.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    class ModuleInformation
    {
        public string ModuleName { get; set; }
        public SourceCodeIndexer CodeIndexer { get; set; }
        public string Origin { get; set; }
    }
}
