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

        private bool _initialPromptPrinted;

        public BeforeExecutionState(OscriptDebugController controller)
        {
            this._controller = controller;
            Prompt = "init";

            FillCommands();
        }

        private void FillCommands()
        {
            AddCommand(new DebuggerCommandDescription()
            {
                Token = "bp",
                Command = DebuggerCommands.SetBreakpoint,
                HelpString = "Создание точки останова. Аргументы:" +
                             "\n <путь к файлу>" +
                             "\n <номер строки в файле>" +
                             "\n\nВозвращаемое значение:" +
                             "\n Число - идентификатор установленной точки. -1 если не удалось установить точку",
                Action = SetBpAction

            });

            AddCommand(new DebuggerCommandDescription()
            {
                Token = "help",
                Command = DebuggerCommands.Help,
                HelpString = "Показывает эту справку",
                Action = PrintHelp
            });
        }

        private void SetBpAction(string[] args)
        {
            Output.WriteLine("SetBP - dummy execution");
        }

        public override void Enter()
        {
            if (!_initialPromptPrinted)
            { 
                Output.WriteLine("Режим ожидания запуска. Ни одна строка кода еще не выполнена.\n" +
                                 "Для просмотра списка доступных команд введите help\n" +
                                 "Для запуска программы введите run\n");

                _initialPromptPrinted = true;
            }

        _controller.InputCommand();
        }

        private void PrintHelp(string[] args)
        {
            Output.WriteLine("Доступные команды:");
            foreach (var cmdDescr in Commands)
            {
                Output.WriteLine(cmdDescr.Token);
            }
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
