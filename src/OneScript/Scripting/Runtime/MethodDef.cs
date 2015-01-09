using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Runtime
{
    public class MethodDef
    {
        public MethodDef()
        {
            VariableRefs = new List<SymbolBinding>();
        }

        public string Name { get; set; }
        
        public MethodSignatureData Signature { get; set; }

        public bool IsExported { get; set; }

        public int EntryPoint { get; set; }

        public List<SymbolBinding> VariableRefs { get; private set; }
    }
}
