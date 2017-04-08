using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
   
    enum StopKind
    {
        SourceLine,
        MethodEntry,
        MethodReturn
    }

    struct SourceLineStop
    {
        public string source;
        public int line;
        public bool isPersisted;
    }


    class MachineStopManager
    {
    }
}
