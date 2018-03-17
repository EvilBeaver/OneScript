/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;

using OneScript.DebugProtocol.FSM;

namespace oscript.DebugServer
{
    internal class BeforeExecutionState : StoppedState
    {
        private bool _initialPromptPrinted;

        public BeforeExecutionState(InteractiveDebugController controller) : base(controller)
        {
            Prompt = "init";
        }

        public override void Enter()
        {
            // уберем команды, которые не сработают до начала выполнения первой строчки
            var newCommands = Commands.Where(x => x.Command != "next" && !x.Command.StartsWith("step")).ToArray();
            Commands.Clear();
            Commands.AddRange(newCommands);

            Output.WriteLine("Режим ожидания запуска. Ни одна строка кода еще не выполнена.\n" +
                                 "Для просмотра списка доступных команд введите help\n" +
                                 "Для запуска программы введите run\n" +
                                 "Выход: exit\n");

            base.Enter();
        }
    }
}
