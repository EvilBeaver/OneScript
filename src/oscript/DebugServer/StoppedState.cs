using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal class StoppedState : ConsoleDebuggerState
    {
        public StoppedState(InteractiveDebugController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            Controller.WaitForDebugEvent(DebugEventType.Continue);
        }
    }
}