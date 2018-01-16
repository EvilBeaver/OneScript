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