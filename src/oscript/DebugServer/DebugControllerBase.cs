/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Threading;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal abstract class DebugControllerBase : IDebugController
    {
        protected ManualResetEventSlim DebugCommandEvent { get; } = new ManualResetEventSlim();
        
        public abstract void WaitForDebugEvent(DebugEventType theEvent);

        public abstract void NotifyProcessExit(int exitCode);

        public virtual void OnMachineReady(MachineInstance instance)
        {
            instance.MachineStopped += MachineStopHandler;
        }

        private void MachineStopHandler(object sender, MachineStoppedEventArgs e)
        {
            OnMachineStopped((MachineInstance)sender, e.Reason);
        }

        protected abstract void OnMachineStopped(MachineInstance machine, MachineStopReason reason);
        
    }
}
