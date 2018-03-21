/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;

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
                Command = "threads",
                HelpString = "Список потоков, выполняющих скрипты",
                Action = GetThreadsAction

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
                HelpString = "Запуск потока выполнения\n\t[номер-потока] - необязательный номер потока, который нужно стартовать",
                Action = (args) => { }
            });
        }

        private void GetThreadsAction(object[] obj)
        {
            foreach (var threadId in Controller.GetAllThreadIds())
            {
                Output.WriteLine(threadId.ToString());
            }
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
            var argsWalk = GetArgsWalker(args);
            var srcLine = argsWalk.Next();
            var lineNumberStr = argsWalk.Next();
            if (srcLine == null || lineNumberStr == null)
            {
                PrintHelp(new object[]{"bp"});
                return;
            }

            int lineNumber;
            if (!Int32.TryParse(lineNumberStr, out lineNumber))
            {
                Output.WriteLine("Некорректный номер строки");
            }

            foreach (var token in Controller.GetAllTokens())
            {
                token.Machine.SetBreakpoint(srcLine, lineNumber, out var bpId);
                Output.WriteLine("Установлена точка " + bpId);
            }
        }

        private static CmdLineHelper GetArgsWalker(object[] args)
        {
            return new CmdLineHelper(args.Cast<string>().ToArray());
        }

        public override void Enter()
        {
            Controller.InputCommand();
        }

        private void PrintHelp(object[] args)
        {
            if (args.Length == 0)
            {
                Output.WriteLine("Доступные команды:");
                foreach (var cmdDescr in Commands)
                {
                    Output.WriteLine(cmdDescr.Command);
                }
            }
            else
            {
                var argsWalk = GetArgsWalker(args);
                var cmdName = argsWalk.Next();
                var command = Commands.Find(x => x.Command == cmdName) as ConsoleDebugCommandDescription;
                if (command == null)
                {
                    Output.WriteLine($"Неизвестная команда {cmdName}");
                }
                else
                {
                    Output.WriteLine(command.HelpString);
                }
            }
        }
    }
}
