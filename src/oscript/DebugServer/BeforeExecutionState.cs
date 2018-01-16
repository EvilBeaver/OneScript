/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.DebugProtocol.FSM;

namespace oscript.DebugServer
{
    internal class BeforeExecutionState : ConsoleDebuggerState
    {
        private bool _initialPromptPrinted;

        public BeforeExecutionState(InteractiveDebugController controller) : base(controller)
        {
            Prompt = "init";

            FillCommands();
        }

        private void FillCommands()
        {
            AddCommand(new ConsoleDebugCommandDescription()
            {
                Command = "bp",
                HelpString = "Создание точки останова. Аргументы:" +
                             "\n <путь к файлу>" +
                             "\n <номер строки в файле>" +
                             "\n\nВозвращаемое значение:" +
                             "\n Число - идентификатор установленной точки. -1 если не удалось установить точку",
                Action = SetBpAction

            });

            AddCommand(new ConsoleDebugCommandDescription()
            {
                Command = "help",
                HelpString = "Показывает эту справку",
                Action = PrintHelp
            });

            AddCommand(new ConsoleDebugCommandDescription()
            {
                Command = "exit",
                HelpString = "Выход из отладки",
                Action = ExitDebugger
            });

            AddCommand(new ConsoleDebugCommandDescription()
            {
                Command = "run",
                HelpString = "Запуск потока выполнения",
                Action = (args) => { }
            });
        }

        private void ExitDebugger(object[] obj)
        {
            
        }

        private void SetBpAction(object[] args)
        {
            Output.WriteLine("SetBP - dummy execution");
        }

        public override void Enter()
        {
            if (!_initialPromptPrinted)
            { 
                Output.WriteLine("Режим ожидания запуска. Ни одна строка кода еще не выполнена.\n" +
                                 "Для просмотра списка доступных команд введите help\n" +
                                 "Для запуска программы введите run\n" +
                                 "Выход: exit\n");

                _initialPromptPrinted = true;
            }

            Controller.InputCommand();
        }

        private void PrintHelp(object[] args)
        {
            Output.WriteLine("Доступные команды:");
            foreach (var cmdDescr in Commands)
            {
                Output.WriteLine(cmdDescr.Command);
            }
        }
    }
}
