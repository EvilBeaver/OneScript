/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language;

namespace ScriptEngine.Machine
{
    class ExecutionFrame
    {
        public IVariable[] Locals;
        public int InstructionPointer;
        public int LineNumber;
        public bool DiscardReturnValue;
        public string MethodName;
        public ScriptException LastException;
        public LoadedModule Module;
        public bool IsReentrantCall;
        
        public Stack<IValue> LocalFrameStack = new Stack<IValue>();


        public Scope ModuleScope { get; set; }
        public int ModuleLoadIndex { get; set; }

        public override string ToString()
        {
            return $"{MethodName}: {LineNumber} ({Module.Source.Name})";
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
