/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Tasks
{
    [ContextClass("МенеджерФоновыхЗаданий", "BackgroundTasksManager")]
    public class BackgroundTasksManager : AutoContext<BackgroundTasksManager>, IDisposable
    {
        private readonly ScriptingEngine _engine;
        private List<BackgroundTask> _tasks = new List<BackgroundTask>();

        public BackgroundTasksManager(ScriptingEngine engine)
        {
            _engine = engine;
        }
        
        /// <summary>
        /// Создать и стартовать задание
        /// </summary>
        /// <param name="target">Объект, метод которого нужно выполнить</param>
        /// <param name="methodName">Имя экспортного метода в объекте</param>
        /// <param name="parameters">Массив параметров метода</param>
        /// <returns>ФоновоеЗадание</returns>
        [ContextMethod("Создать", "Create")]
        public BackgroundTask Create(IRuntimeContextInstance target, string methodName, ArrayImpl parameters = null)
        {
            var task = new BackgroundTask(target, methodName, parameters);
            _tasks.Add(task);

            var worker = new Task(() =>
            {
                if(!MachineInstance.Current.IsRunning)
                    _engine.Environment.LoadMemory(MachineInstance.Current);
                
                task.ExecuteOnCurrentThread();
                
            }, TaskCreationOptions.LongRunning);

            task.WorkerTask = worker;
            worker.Start();
            
            return task;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _tasks.Clear();
        }
        
        /// <summary>
        /// Ждать завершения всех незавершенных заданий
        /// </summary>
        [ContextMethod("ЖдатьВсе", "WaitAll")]
        public void WaitAll()
        {
            var tasks = _tasks
                .Select(x => x.WorkerTask)
                .ToArray();

            // Фоновые задания перехватывают исключения внутри себя 
            // и выставляют свойство ИнформацияОбОшибке
            // если WaitAll выбросит исключение, значит действительно что-то пошло не так на уровне самого Task
            Task.WaitAll(tasks);
        }

        [ContextMethod("Получить", "Get")]
        public ArrayImpl Get()
        {
            return new ArrayImpl(_tasks.ToArray());
        }

        public void Dispose()
        {
            WaitAll();
            _tasks.Clear();
        }
    }
}