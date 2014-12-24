using OneScript.Scripting.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Runtime
{
    public class ModuleImage
    {
        public ModuleImage()
        {
            EntryMethodIndex = INVALID_ADDRESS;
            VariableRefs = new List<SymbolBinding>();
            MethodRefs = new List<SymbolBinding>();
            Constants = new List<ConstDefinition>();
            Variables = new List<VariableDefinition>();
            Methods = new List<MethodDefinition>();
            Code = new List<Command>();
        }

        public const int INVALID_ADDRESS = -1;

        public int EntryMethodIndex { get; set; }
        public IList<SymbolBinding> VariableRefs { get; private set; }
        public IList<SymbolBinding> MethodRefs { get; private set; }
        public IList<ConstDefinition> Constants { get; private set; }
        public IList<VariableDefinition> Variables { get; private set; }
        public IList<MethodDefinition> Methods { get; private set; }
        public IList<Command> Code { get; private set; }
        public ISourceCodeIndexer CodeIndexer { get; set; }
        
    }

    public struct MethodDefinition
    {
        public int EntryPoint;
        public string Name;
        public int VariableFrameSize;
        public bool IsExported;
        public MethodSignatureData Signature;
    }

    public struct VariableDefinition
    {
        public string Name;
        public bool IsExported;
    }
}
