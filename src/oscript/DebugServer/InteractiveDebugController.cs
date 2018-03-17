/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
        protected ManualResetEventSlim DebugCommandEvent { get; } = new ManualResetEventSlim();

        public InteractiveDebugController()
        {
            DebugFsm = new DebuggerFSM();
            var initialState = new BeforeExecutionState(this);
            var runningState = new RunningState(this);
            var stoppedState = new StoppedState(this);

            // from initial
            DebugFsm.AddTransition(initialState, "run", runningState);
            DebugFsm.AddTransition(initialState, "help", initialState);
            DebugFsm.AddTransition(initialState, "bp", initialState);
            DebugFsm.AddTransition(initialState, "exit", new FinalState(this));
            // from stopped
            DebugFsm.AddTransition(stoppedState, "run", runningState);
            DebugFsm.AddTransition(stoppedState, "help", stoppedState);
            DebugFsm.AddTransition(stoppedState, "bp", stoppedState);
            DebugFsm.AddTransition(initialState, "exit", new FinalState(this));

        }

        public DebuggerFSM DebugFsm { get; }

        public override void Wait()
        {
            DebugFsm.Start();
            base.Wait();
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
            while (true)
            {
                Output.Write($"{((ConsoleDebuggerState)DebugFsm.CurrentState).Prompt}>");
                var line = Console.ReadLine();
                var parser = new CmdLineHelper(SplitArguments(line));
                var commandName = parser.Next();
                try
                {
                    DebugFsm.DispatchCommand(commandName, parser.Tail());
                    break;
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
