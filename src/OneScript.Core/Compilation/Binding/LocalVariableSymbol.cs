using System;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public class LocalVariableSymbol : IVariableSymbol
    {
        public LocalVariableSymbol(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        
        public LocalVariableSymbol(string name)
        {
            Name = name;
            Type = typeof(BslValue);
        }

        public string Name { get; }
        
        public string Alias => null;
        
        public Type Type { get; }
    }
}