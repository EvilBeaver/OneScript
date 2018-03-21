/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
            
        }
    }
}
