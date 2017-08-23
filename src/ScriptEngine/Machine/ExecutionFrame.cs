/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
        public LoadedModule Module;
        public bool IsReentrantCall;
        
        public Stack<IValue> LocalFrameStack = new Stack<IValue>();


        public Scope ModuleScope { get; set; }

        public override string ToString()
        {
            return $"{MethodName}: {LineNumber} ({Module.ModuleInfo.ModuleName})";
        }
    }

    public struct ExecutionFrameInfo
    {
        public string Source;
        public int LineNumber;
        public string MethodName;

        internal ExecutionFrame FrameObject;
    }
}
