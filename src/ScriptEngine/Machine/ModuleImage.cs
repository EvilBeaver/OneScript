using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    [Serializable]
    class ModuleImage : IModuleImage
    {
        public ModuleImage()
        {
            EntryMethodIndex = -1;
            Code = new List<Command>();
            VariableRefs = new List<SymbolBinding>();
            MethodRefs = new List<SymbolBinding>();
            Methods = new List<MethodDescriptor>();
            Constants = new List<ConstDefinition>();
            ExportedProperties = new List<ExportedSymbol>();
            ExportedMethods = new List<ExportedSymbol>();
        }
        
        public int VariableFrameSize { get; set; }
        public int EntryMethodIndex { get; set; }
        public IList<Command> Code { get; set; }
        public IList<SymbolBinding> VariableRefs { get; set; }
        public IList<SymbolBinding> MethodRefs { get; set; }
        public IList<MethodDescriptor> Methods { get; set; }
        public IList<ConstDefinition> Constants { get; set; }
        public IList<ExportedSymbol> ExportedProperties { get; set; }
        public IList<ExportedSymbol> ExportedMethods { get; set; }
    }
}
