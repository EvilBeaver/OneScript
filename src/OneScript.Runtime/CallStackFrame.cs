using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Runtime.Compiler;

namespace OneScript.Runtime
{
    public class CallStackFrame : ICallStackFrame
    {
        private ModuleMethodDefinition _method;
        private CompiledModule _module;

        public CallStackFrame(CompiledModule module, ModuleMethodDefinition method)
        {
            _method = method;
            _module = module;
            InstructionPointer = method.EntryPoint;
            Locals = new IValueRef[method.VariableTable.Count];
            for (int i = 0; i < Locals.Length; i++)
            {
                Locals[i] = new GeneralValueRef();
            }
        }

        public IValueRef[] Locals
        {
            get;
            private set;
        }

        public int CurrentLine
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CurrentMethod
        {
            get
            {
                return _method.Name;
            }
        }

        public int InstructionPointer { get; internal set; }

        public ICompiledModule Module
        {
            get
            {
                return _module;
            }
        }

        public CompiledModule ModuleInternal
        {
            get
            {
                return _module;
            }
        }
    }
}
