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
using OneScript.Commons;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Tasks
{
    [ContextClass("МенеджерФоновыхЗаданий", "BackgroundTasksManager")]
    public class BackgroundTasksManager : AutoContext<BackgroundTasksManager>, IDisposable
    {
        private readonly MachineEnvironment _runtimeContext;
        private List<BackgroundTask> _tasks = new List<BackgroundTask>();

        public BackgroundTasksManager(MachineEnvironment runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }
        
        /// <summary>
        /// Создать и стартовать задание
        /// </summary>
        /// <param name="target">Объект, метод которого нужно выполнить</param>
        /// <param name="methodName">Имя экспортного метода в объекте</param>
        /// <param name="parameters">Массив параметров метода</param>
        /// <returns>ФоновоеЗадание</returns>
        [ContextMethod("Выполнить", "Execute")]
        public BackgroundTask Execute(IRuntimeContextInstance target, string methodName, ArrayImpl parameters = null)
        {
            var task = new BackgroundTask(target, methodName, parameters);
            _tasks.Add(task);

            var worker = new Task(() =>
            {
                MachineInstance.Current.SetMemory(_runtimeContext);
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
        /// Ожидает завершения всех переданных заданий
        /// </summary>
        /// <param name="tasks">Массив заданий</param>
        /// <param name="timeout">Таймаут ожидания. 0 = ожидать бесконечно</param>
        /// <returns>Истина - дождались все задания, Ложь - истек таймаут</returns>
        [ContextMethod("ОжидатьВсе", "WaitAll")]
        public bool WaitAll(ArrayImpl tasks, int timeout = 0)
        {
            var workers = GetWorkerTasks(tasks);
            timeout = ConvertTimeout(timeout);
            
            // Фоновые задания перехватывают исключения внутри себя 
            // и выставляют свойство ИнформацияОбОшибке
            // если WaitAll выбросит исключение, значит действительно что-то пошло не так на уровне самого Task
            return Task.WaitAll(workers, timeout);
        }
        
        /// <summary>
        /// Ожидать хотя бы одно из переданных заданий.
        /// </summary>
        /// <param name="tasks">Массив заданий</param>
        /// <param name="timeout">Таймаут ожидания. 0 = ожидать бесконечно</param>
        /// <returns>Число. Индекс в массиве заданий, указывающий на элемент-задание, которое завершилось. -1 = сработал таймаут</returns>
        [ContextMethod("ОжидатьЛюбое", "WaitAny")]
        public int WaitAny(ArrayImpl tasks, int timeout = 0)
        {
            var workers = GetWorkerTasks(tasks);
            timeout = ConvertTimeout(timeout);
            
            // Фоновые задания перехватывают исключения внутри себя 
            // и выставляют свойство ИнформацияОбОшибке
            // если WaitAny выбросит исключение, значит действительно что-то пошло не так на уровне самого Task
            return Task.WaitAny(workers, timeout);
        }

        /// <summary>
        /// Блокирует поток до завершения всех заданий.
        /// Выбрасывает исключение, если какие-то задания завершились аварийно.
        /// Выброшенное исключение в свойстве Параметры содержит массив аварийных заданий.
        /// </summary>
        [ContextMethod("ОжидатьЗавершенияЗадач", "WaitCompletionOfTasks")]
        public void WaitCompletionOfTasks()
        {
            lock (_tasks)
            {
                var workers = GetWorkerTasks();
                Task.WaitAll(workers);

                var failedTasks = _tasks.Where(x => x.State == TaskStateEnum.CompletedWithErrors)
                    .ToList();
                
                if (failedTasks.Any())
                {
                    throw new ParametrizedRuntimeException(
                        Locale.NStr("ru = 'Задания завершились с ошибками';en = 'Tasks are completed with errors'"),
                        new ArrayImpl(failedTasks));
                }
                
                _tasks.Clear();
            }
        }

        [ContextMethod("ПолучитьФоновыеЗадания", "GetBackgroundJobs")]
        public ArrayImpl GetBackgroundJobs(StructureImpl filter = default)
        {
            if(filter == default)
                return new ArrayImpl(_tasks.ToArray());

            var arr = new ArrayImpl();
            foreach (var task in _tasks)
            {
                var result = true;
                foreach (var filterItem in filter)
                {
                    switch (filterItem.Key.AsString().ToLower())
                    {
                        case "состояние":
                        case "state":
                            var enumval = filterItem.Value as ClrEnumValueWrapper<TaskStateEnum>;
                            if(enumval == default)
                                continue;

                            result = result && task.State == enumval.UnderlyingValue;
                            break;
                        
                        case "имяметода":
                        case "methodname":
                            result = result && task.MethodName.ToLower() == filterItem.Value.AsString();
                            break;
                        
                        case "объект":
                        case "object":
                            result = result && task.Target.Equals(filterItem.Value);
                            break;
                        
                        case "уникальныйидентификатор":
                        case "uuid":
                            result = result && task.Identifier.Equals(filterItem.Value);
                            break;
                    }
                }
                
                if(result)
                    arr.Add(task);
            }

            return arr;
        }

        [ContextMethod("ПолучитьТекущее", "GetCurrent")]
        public IValue GetCurrent()
        {
            var currentId = Task.CurrentId;
            if (currentId == null)
                return ValueFactory.Create();

            var task = _tasks.FirstOrDefault(x => x.TaskId == (int) currentId && x.State == TaskStateEnum.Running);
            return task ?? ValueFactory.Create();
        }

        internal static int ConvertTimeout(int timeout)
        {
            if(timeout < 0)
                throw RuntimeException.InvalidArgumentValue();

            return timeout == 0 ? Timeout.Infinite : timeout;
        }

        private static Task[] GetWorkerTasks(ArrayImpl tasks)
        {
            return tasks
                .Cast<BackgroundTask>()
                .Select(x => x.WorkerTask)
                .ToArray();
        }

        private static Task[] GetWorkerTasks(IEnumerable<BackgroundTask> tasks)
        {
            return tasks.Select(x => x.WorkerTask).ToArray();
        }

        private Task[] GetWorkerTasks()
        {
            return GetWorkerTasks(_tasks);
        }

        public void Dispose()
        {
            Task.WaitAll(GetWorkerTasks());
            _tasks.Clear();
        }

        [ScriptConstructor]
        public static BackgroundTasksManager Create(TypeActivationContext context)
        {
            return new BackgroundTasksManager(context.Services.Resolve<MachineEnvironment>());
        }
    }
}