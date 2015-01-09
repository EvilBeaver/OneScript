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
            LocalRefs = new List<SymbolBinding>();
        }

        public string Name { get; set; }
        
        public MethodSignatureData Signature { get; set; }

        public bool IsExported { get; set; }

        public int EntryPoint { get; set; }

        public List<SymbolBinding> LocalRefs { get; private set; }
    }
}
