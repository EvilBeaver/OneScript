using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    interface IAttachableContext
    {
        void OnAttach(MachineInstance machine,
            out IVariable[] variables, 
            out MethodInfo[] methods, 
            out IRuntimeContextInstance instance);
    }
}
