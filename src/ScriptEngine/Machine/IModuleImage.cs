using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    interface IModuleImage
    {
        int VariableFrameSize { get; set; }
        int EntryMethodIndex { get; set; }
        IList<Command> Code { get; }
        IList<SymbolBinding> VariableRefs { get; }
        IList<SymbolBinding> MethodRefs { get; }
        IList<MethodDescriptor> Methods { get; }
        IList<ConstDefinition> Constants { get; }
        IList<ExportedSymbol> ExportedProperties { get; }
        IList<ExportedSymbol> ExportedMethods { get; }
    }

    [Serializable]
    struct MethodDescriptor
    {
        public MethodInfo Signature;
        public int VariableFrameSize;
        public int EntryPoint;
    }

    [Serializable]
    struct ExportedSymbol
    {
        public string SymbolicName;
        public int Index;
    }

}
