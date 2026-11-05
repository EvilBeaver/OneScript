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
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;

namespace OneScript.StandardLibrary.Tasks
{
    [ContextClass("МенеджерФоновыхЗаданий", "BackgroundTasksManager")]
    public class BackgroundTasksManager : AutoContext<BackgroundTasksManager>, IDisposable
    {
        /// <summary>
        /// Сколько заданий менеджер хранит, если в настройке backgroundJobs.capacity не указано иное.
        /// Как у кластера 1С:Предприятия
        /// </summary>
        public const int DefaultCapacity = 1000;

        private readonly ExecutionContext _runtimeContext;
        private readonly BackgroundTasksRegistry _tasks;

        public BackgroundTasksManager(ExecutionContext runtimeContext)
            : this(runtimeContext, runtimeContext.Services.TryResolve<BackgroundTasksOptions>()?.Capacity ?? DefaultCapacity)
        {
        }

        public BackgroundTasksManager(ExecutionContext runtimeContext, int capacity)
        {
            if (capacity <= 0)
                throw RuntimeException.InvalidArgumentValue();

            _runtimeContext = runtimeContext;
            _tasks = new BackgroundTasksRegistry(capacity);
        }
        
        /// <summary>
        /// Создать и стартовать задание
        /// </summary>
        /// <param name="target">Объект, метод которого нужно выполнить</param>
        /// <param name="methodName">Имя экспортного метода в объекте</param>
        /// <param name="parameters">Массив параметров метода</param>
        /// <param name="longRunning">Задание выполняется длительное время и требует себе отдельный поток (не использует пул потоков)</param>
        /// <returns>ФоновоеЗадание</returns>
        [ContextMethod("Выполнить", "Execute")]
        public BackgroundTask Execute(IRuntimeContextInstance target, string methodName, ArrayImpl parameters = null, bool longRunning = false)
        {
            var task = new BackgroundTask(target, methodName, parameters)
            {
                Parent = FindCurrentTask()
            };

            // PreferFairness - в общую очередь пула: иначе ожидающий из потока пула (запрос веб-сервера)
            // может выполнить задание прямо в своем потоке, и два процесса поделят блокировки потока
            var taskCreationOptions = longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.PreferFairness;
            var worker = new Task(() =>
            {
                try
                {
                    var process = _runtimeContext.Services.Resolve<IBslProcessFactory>().NewProcess(task.CancellationToken);
                    task.ExecuteOnCurrentThread(process);
                }
                finally
                {
                    // До завершения рабочей задачи: вытесняются задания, завершившиеся раньше,
                    // и ожидающий завершения уже видит задание на новом месте
                    _tasks.MarkCompleted(task);
                }

            }, taskCreationOptions);

            task.WorkerTask = worker;
            _tasks.TryAdd(task.TaskId, task);
            worker.Start();
            
            return task;
        }

        /// <summary>
        /// Удаляет все задания из списка заданий менеджера.
        /// Выполняющиеся задания не останавливаются, но их больше не вернут ПолучитьФоновыеЗадания
        /// и ПолучитьТекущее, и их не будет ждать ОжидатьЗавершенияЗадач.
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _tasks.Clear();
        }
        
        /// <summary>
        /// Ожидает завершения всех переданных заданий
        /// </summary>
        /// <param name="process">Текущий процесс, в котором вызван данный метод</param>
        /// <param name="tasks">Массив заданий</param>
        /// <param name="timeout">Таймаут ожидания. 0 = ожидать бесконечно</param>
        /// <returns>Истина - дождались все задания, Ложь - истек таймаут</returns>
        [ContextMethod("ОжидатьВсе", "WaitAll")]
        public bool WaitAll(IBslProcess process, ArrayImpl tasks, int timeout = 0)
        {
            var workers = GetWorkerTasks(tasks);
            timeout = ConvertTimeout(timeout);
            
            // Фоновые задания перехватывают исключения внутри себя 
            // и выставляют свойство ИнформацияОбОшибке
            // если WaitAll выбросит исключение, значит действительно что-то пошло не так на уровне самого Task
            return Task.WaitAll(workers, timeout, process.CancellationToken);
        }
        
        /// <summary>
        /// Ожидать хотя бы одно из переданных заданий.
        /// </summary>
        /// <param name="process">Текущий процесс, в котором вызван данный метод</param>
        /// <param name="tasks">Массив заданий</param>
        /// <param name="timeout">Таймаут ожидания. 0 = ожидать бесконечно</param>
        /// <returns>Число. Индекс в массиве заданий, указывающий на элемент-задание, которое завершилось. -1 = сработал таймаут</returns>
        [ContextMethod("ОжидатьЛюбое", "WaitAny")]
        public int WaitAny(IBslProcess process, ArrayImpl tasks, int timeout = 0)
        {
            var workers = GetWorkerTasks(tasks);
            timeout = ConvertTimeout(timeout);
            
            // Фоновые задания перехватывают исключения внутри себя 
            // и выставляют свойство ИнформацияОбОшибке
            // если WaitAny выбросит исключение, значит действительно что-то пошло не так на уровне самого Task
            return Task.WaitAny(workers, timeout, process.CancellationToken);
        }

        /// <summary>
        /// Блокирует поток до завершения всех заданий.
        /// Вызванный из фонового задания, ждет только задания, запущенные из него (и из них дальше).
        /// Выбрасывает исключение, если какие-то задания завершились аварийно.
        /// Выброшенное исключение в свойстве Параметры содержит массив аварийных заданий.
        /// Учитываются только задания, которые еще есть в списке менеджера: завершенные задания,
        /// в том числе аварийные, при переполнении из него вытесняются. Если заданий больше,
        /// чем хранит менеджер, храните ссылки на них и ждите через ОжидатьВсе, проверяя ИнформацияОбОшибке.
        /// </summary>
        [ContextMethod("ОжидатьЗавершенияЗадач", "WaitCompletionOfTasks")]
        public void WaitCompletionOfTasks(IBslProcess process)
        {
            // Родитель и соседние задания могут сами ждать текущее - их ждать нельзя
            var current = FindCurrentTask();
            var snapshot = _tasks.Values.Where(x => current == null || x.IsDescendantOf(current)).ToArray();
            Task.WaitAll(GetWorkerTasks(snapshot), process.CancellationToken);

            var failedTasks = snapshot.Where(x => x.State == TaskStateEnum.CompletedWithErrors)
                .ToList();
            
            if (failedTasks.Count != 0)
            {
                throw new ParametrizedRuntimeException(
                    Locale.NStr("ru = 'Задания завершились с ошибками';en = 'Tasks are completed with errors'"),
                    new ArrayImpl(failedTasks));
            }

            foreach (var task in snapshot)
            {
                _tasks.TryRemove(task.TaskId, out _);
            }
        }

        /// <summary>
        /// Возвращает задания менеджера: выполняющиеся в порядке запуска, завершенные в порядке завершения.
        /// Если заданий больше, чем задано настройкой backgroundJobs.capacity (по умолчанию 1000),
        /// из списка убираются задания, завершившиеся раньше всех. Выполняющиеся задания остаются
        /// в списке, даже если их больше этого числа.
        /// </summary>
        /// <param name="filter">Структура отбора: Состояние, ИмяМетода, Объект, УникальныйИдентификатор</param>
        /// <returns>Массив заданий</returns>
        [ContextMethod("ПолучитьФоновыеЗадания", "GetBackgroundJobs")]
        public ArrayImpl GetBackgroundJobs(StructureImpl filter = default)
        {
            if(filter == default)
                return new ArrayImpl(_tasks.Values);

            var arr = new ArrayImpl();
            foreach (var task in _tasks.Values)
            {
                var result = true;
                foreach (var filterItem in filter)
                {
                    switch (filterItem.Key.ToString()!.ToLower())
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
                            result = result && task.MethodName.ToLower() == filterItem.Value.ToString();
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

        /// <summary>
        /// Возвращает фоновое задание, в котором выполняется текущий код.
        /// </summary>
        /// <returns>ФоновоеЗадание или Неопределено, если код выполняется не в фоновом задании этого менеджера</returns>
        [ContextMethod("ПолучитьТекущее", "GetCurrent")]
        public IValue GetCurrent()
        {
            return (IValue)FindCurrentTask() ?? ValueFactory.Create();
        }

        // Фоновое задание, в котором выполняется код, или null
        private BackgroundTask FindCurrentTask()
        {
            var currentId = Task.CurrentId;
            if (currentId == null)
                return null;

            if (_tasks.TryGetValue(currentId.Value, out var task) && task.State == TaskStateEnum.Running)
                return task;

            return null;
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
            return GetWorkerTasks(_tasks.Values);
        }

        public void Dispose()
        {
            Task.WaitAll(GetWorkerTasks());
            _tasks.Clear();
        }
    }
}