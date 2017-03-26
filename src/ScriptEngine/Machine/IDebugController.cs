using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public interface IDebugController
    {
        void WaitForDebugEvent(DebugEvent theEvent);

        void NotifyProcessExit();
    }
}
