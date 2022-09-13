using System;
using OneScript.Compilation.Binding;

namespace OneScript.Native.Compiler
{
    public class LocalVariableSymbol : IVariableSymbol
    {
        public string Name { get; set;  }
        public string Alias { get; set; }
        
        public Type Type { get; set;  }
    }
}