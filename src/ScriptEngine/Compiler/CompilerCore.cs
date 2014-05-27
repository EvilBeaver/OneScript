using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Compiler
{
    public struct VariableDescriptor
    {
        public string Identifier;
        public SymbolType Type;
    }

    public enum SymbolType
    {
        Variable,
        ContextProperty
    }

    struct VariableInfo
    {
        public int Index;
        public SymbolType Type;
    }

    struct VariableBinding
    {
        public SymbolType type;
        public SymbolBinding binding;
    }

}
