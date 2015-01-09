using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Scripting.Compiler;

namespace OneScript.Scripting.Runtime
{
    public class CompiledModule
    {
        private List<VariableDef> _variables = new List<VariableDef>();
        private List<MethodDef> _methods = new List<MethodDef>(); 
        private List<SymbolBinding> _variableRefs = new List<SymbolBinding>();
        private List<SymbolBinding> _methodRefs = new List<SymbolBinding>();
        private List<ConstDefinition> _constants = new List<ConstDefinition>();
        private List<Command> _code = new List<Command>(); 

        public List<VariableDef> Variables
        {
            get { return _variables; }
        }

        public List<MethodDef> Methods
        {
            get { return _methods; }
        }

        public List<SymbolBinding> VariableRefs
        {
            get { return _variableRefs; }
        }

        public List<SymbolBinding> MethodRefs
        {
            get { return _methodRefs; }
        }

        public List<ConstDefinition> Constants
        {
            get { return _constants; }
        }

        public List<Command> Code
        {
            get { return _code; }
        }

        public int EntryMethodIndex { get; set; }
    }
}
