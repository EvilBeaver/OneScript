using System;

using OneScript.DebugProtocol.FSM;

namespace oscript.DebugServer
{
    internal class RunningState : ConsoleDebuggerState
    {
        public RunningState(InteractiveDebugController controller) : base(controller)
        {
            AddCommand(new DebuggerCommandDescription()
            {
                Action = StopEventHandler,
                Command = "break"
            });
        }

        private void StopEventHandler(object[] obj)
        {
            Output.WriteLine("Machine stopped: " + obj);
        }

        public override void Enter()
        {
            throw new NotImplementedException();
            //Controller.Execute();
        }
    }
}