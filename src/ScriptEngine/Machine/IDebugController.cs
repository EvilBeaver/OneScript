
namespace ScriptEngine.Machine
{
    public interface IDebugController
    {
        void WaitForDebugEvent(DebugEventType theEvent);

        void OnMachineReady(MachineInstance instance);

        void NotifyProcessExit();
        
    }
}
