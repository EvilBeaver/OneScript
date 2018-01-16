using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal class InteractiveDebugController : DebugControllerBase
    {
        public InteractiveDebugController()
        {
            DebugFsm = new DebuggerFSM();
            var initialState = new BeforeExecutionState(this);
            var runningState = new RunningState(this);
            var stoppedState = new StoppedState(this);

            DebugFsm.AddTransition(initialState, "run", runningState);
            DebugFsm.AddTransition(initialState, "help", initialState);
            DebugFsm.AddTransition(initialState, "bp", initialState);
            DebugFsm.AddTransition(initialState, "exit", initialState); // выход пока не проработан
            DebugFsm.AddTransition(runningState, "break", stoppedState);
        }

        public DebuggerFSM DebugFsm { get; }

        public override void WaitForDebugEvent(DebugEventType theEvent)
        {
            throw new NotImplementedException();
        }

        public override void NotifyProcessExit(int exitCode)
        {
            throw new NotImplementedException();
        }

        protected override void OnMachineStopped(MachineInstance machine, MachineStopReason reason)
        {
            throw new NotImplementedException();
        }

        public void InputCommand()
        {
            var thread = new Thread(InteractiveInput);
            thread.Start();
        }

        private void InteractiveInput()
        {
            bool selected = false;
            while (!selected)
            {
                Output.Write($"{((ConsoleDebuggerState)DebugFsm.CurrentState).Prompt}>");
                var line = Console.ReadLine();
                var parser = new CmdLineHelper(SplitArguments(line));
                var commandName = parser.Next();
                try
                {
                    DebugFsm.DispatchCommand(commandName, parser.Tail());
                }
                catch (InvalidDebuggerCommandException)
                {
                    Output.WriteLine($"Неизвестная команда {commandName}");
                }
            }

        }

        private string[] SplitArguments(string line)
        {
            return CommandsParser.SplitCommandLine(line);
        }
    }
}
