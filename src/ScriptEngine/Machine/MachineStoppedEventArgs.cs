/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;

namespace ScriptEngine.Machine
{
    public class MachineStoppedEventArgs : EventArgs
    {
        public MachineStoppedEventArgs(MachineStopReason reason)
        {
            Reason = reason;
            ThreadId = Environment.CurrentManagedThreadId;
            ErrorMessage = string.Empty;
        }
        
        public MachineStoppedEventArgs(MachineStopReason reason, int threadId, string errorMessage = "")
        {
            Reason = reason;
            ThreadId = threadId;
            ErrorMessage = errorMessage;
        }

        public MachineStopReason Reason { get; }
        
        public int ThreadId { get; }

        public string ErrorMessage { get; }
    }

    public enum MachineStopReason
    {
        Breakpoint,
        BreakpointConditionError,
        Step,
        Exception
    }
}
