using OneScript.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class MethodDef
    {
        public MethodDef()
        {
            Locals = new List<VariableDef>();
        }

        public string Name { get; set; }
        
        public MethodSignatureData Signature { get; set; }

        public bool IsExported { get; set; }

        public int EntryPoint { get; set; }

        public List<VariableDef> Locals { get; private set; }

    }
}
