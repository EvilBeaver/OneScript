using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class ExecutionFrame
    {
        public IVariable[] Locals;
        public int InstructionPointer;
        public int LineNumber;
        public bool DiscardReturnValue;
        public string MethodName;
        public RuntimeException LastException;
        
        public Stack<IValue> LocalFrameStack = new Stack<IValue>();
    }
}
