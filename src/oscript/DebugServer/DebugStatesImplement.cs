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
using System.Threading.Tasks;

namespace oscript.DebugServer
{
    internal class BeforeExecutionState : DebuggerState
    {
        private OscriptDebugController _controller;

        public BeforeExecutionState(OscriptDebugController controller)
        {
            this._controller = controller;
            Prompt = "init";
        }

        public override void ExecuteCommand(DebuggerCommands command, string[] arguments)
        {
            switch (command)
            {
                case DebuggerCommands.Execute:
                    _controller.Execute();
                    break;
                case DebuggerCommands.Help:
                    PrintHelp();
                    break;
                case DebuggerCommands.SetBreakpoint:
                    // dummy
                    Output.WriteLine("bp dummy set");
                    break;
                default:
                    base.ExecuteCommand(command, arguments);
                    break;
            }
        }

        public override void Enter()
        {
            Output.WriteLine("Режим ожидания запуска. Для просмотра списка доступных команд введите help");
            _controller.InputCommand();
        }

        private void PrintHelp()
        {
            Output.WriteLine("Доступные команды:" +
                             "\nbp: <путь к файлу> <номер строки> - установка точки останова" +
                             "\nbprm <номер точки останова> - удаление точки останова");
        }
    }

    internal class RunningState : DebuggerState
    {
        private OscriptDebugController oscriptDebugController;

        public RunningState(OscriptDebugController oscriptDebugController)
        {
            this.oscriptDebugController = oscriptDebugController;
        }
    }

    internal class StoppedState : DebuggerState
    {
        private OscriptDebugController oscriptDebugController;

        public StoppedState(OscriptDebugController oscriptDebugController)
        {
            this.oscriptDebugController = oscriptDebugController;
        }
    }
}
