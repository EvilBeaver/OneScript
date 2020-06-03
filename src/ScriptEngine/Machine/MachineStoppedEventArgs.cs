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
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }
        
        public MachineStoppedEventArgs(MachineStopReason reason, int threadId)
        {
            Reason = reason;
            ThreadId = threadId;
        }
        
        public MachineStopReason Reason { get; }
        
        public int ThreadId { get; }

    }

    public enum MachineStopReason
    {
        Breakpoint,
        Step,
        Exception
    }

}
