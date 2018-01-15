using OneScript.DebugProtocol.FSM;

namespace oscript.DebugServer
{
    internal class RunningState : ConsoleDebuggerState
    {
        public RunningState(OscriptDebugController controller) : base(controller)
        {
            AddCommand(new DebuggerCommandDescription()
            {
                Action = StopEventHandler,
                Command = DebuggerCommands.OutgoingEvent
            });
        }

        private void StopEventHandler(object[] obj)
        {
            Output.WriteLine("Machine stopped: " + obj);
        }

        public override void Enter()
        {
            Controller.Execute();
        }
    }
}