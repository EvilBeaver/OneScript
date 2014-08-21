using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class VariableBlockCompilerState : CompilerState
    {
        public override void Build()
        {
            
        }

        public ModuleImage Module { get; set; }

        public CompilerContext Context { get; set; }
    }
}
