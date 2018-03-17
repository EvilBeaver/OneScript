/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal class StoppedState : ConsoleDebuggerState
    {
        public StoppedState(InteractiveDebugController controller) : base(controller)
        {
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
            Console.WriteLine("Отладка завершена. Нажмите любую клавишу для продолжения");
            Console.ReadKey();

            var tokens = Controller.GetAllTokens();
            foreach (var token in tokens)
            {
                token.ThreadEvent.Set();
            }
            Controller.Dispose();
        }

        private void SetBpAction(object[] args)
        {
            Output.WriteLine("SetBP - dummy execution");
        }

        public override void Enter()
        {
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
