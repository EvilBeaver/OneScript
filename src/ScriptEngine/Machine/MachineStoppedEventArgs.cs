using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class MachineStoppedEventArgs : EventArgs
    {
        public MachineStoppedEventArgs(MachineStopReason reason)
        {
            Reason = reason;
        }
        
        public MachineStopReason Reason { get; }

    }

    public enum MachineStopReason
    {
        Breakpoint
    }

}
