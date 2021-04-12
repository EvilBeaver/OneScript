/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Processes
{
    [GlobalContext(Category = "Работа с процессами операционной системы")]
    public class GlobalProcessesFunctions : GlobalContextBase<GlobalProcessesFunctions>
    {
        /// <summary>
        /// Запуск приложения в операционной системе
        /// </summary>
        /// <param name="cmdLine">Командная строка запуска</param>
        /// <param name="currentDir">Текущая директория запускаемого процесса (необязательно)</param>
        /// <param name="wait">Ожидать завершения (необязательно) по умолчанию Ложь</param>
        /// <param name="retCode">Выходной параметр. Код возврата процесса. Имеет смысл только если указан параметр wait=true</param>
        [ContextMethod("ЗапуститьПриложение", "RunApp")]
        public void RunApp(string cmdLine, string currentDir = null, bool wait = false, [ByRef] IVariable retCode = null)
        {
            var sInfo = ProcessContext.PrepareProcessStartupInfo(cmdLine, currentDir);

            var p = new System.Diagnostics.Process();
            p.StartInfo = sInfo;
            p.Start();

            if(wait)
            {
                p.WaitForExit();
                if(retCode != null)
                    retCode.Value = ValueFactory.Create(p.ExitCode);
            }

        }

        /// <summary>
        /// Создает процесс, которым можно манипулировать из скрипта
        /// </summary>
        /// <param name="cmdLine">Командная строка запуска</param>
        /// <param name="currentDir">Текущая директория запускаемого процесса (необязательно)</param>
        /// <param name="redirectOutput">Перехватывать стандартные потоки stdout и stderr</param>
        /// <param name="redirectInput">Перехватывать стандартный поток stdin</param>
        /// <param name="encoding">Кодировка стандартных потоков вывода и ошибок</param>
        /// <param name="env">Соответствие, где установлены значения переменных среды</param>
        [ContextMethod("СоздатьПроцесс", "CreateProcess")]
        public ProcessContext CreateProcess(string cmdLine, string currentDir = null, bool redirectOutput = false, bool redirectInput = false, IValue encoding = null, MapImpl env = null)
        {
            return ProcessContext.Create(cmdLine, currentDir, redirectOutput, redirectInput, encoding, env);
        }

        /// <summary>
        /// Выполняет поиск процесса по PID среди запущенных в операционной системе
        /// </summary>
        /// <param name="PID">Идентификатор процесса</param>
        /// <returns>Процесс. Если не найден - Неопределено</returns>
        [ContextMethod("НайтиПроцессПоИдентификатору", "FindProcessById")]
        public IValue FindProcessById(int PID)
        {
            System.Diagnostics.Process process;
            try
            {
                process = System.Diagnostics.Process.GetProcessById(PID);
            }
            catch (ArgumentException)
            {
                return ValueFactory.Create();
            }

            return new ProcessContext(process);

        }

        /// <summary>
        /// Выполняет поиск процессов с определенным именем
        /// </summary>
        /// <param name="name">Имя процесса</param>
        /// <returns>Массив объектов Процесс.</returns>
        [ContextMethod("НайтиПроцессыПоИмени", "FindProcessesByName")]
        public IValue FindProcessesByName(string name)
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(name);
            var contextWrappers = processes.Select(x => new ProcessContext(x));

            return new ArrayImpl(contextWrappers);

        }
        
        public static GlobalProcessesFunctions CreateInstance()
        {
            return new GlobalProcessesFunctions();
        }
    }
}