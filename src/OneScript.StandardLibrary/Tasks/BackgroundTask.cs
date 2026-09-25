/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Language;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Tasks
{
    [ContextClass("ФоновоеЗадание", "BackgroundTask")]
    public class BackgroundTask : AutoContext<BackgroundTask>
    {
        private readonly BslMethodInfo _method;
        private readonly int _methIndex;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly object _cancellationLock = new object();
        private bool _cancellationReleased;
        private Task _workerTask;
        private int _taskId;
        
        public BackgroundTask(IRuntimeContextInstance target, string methodName, ArrayImpl parameters = default)
        {
            Target = target;
            MethodName = methodName;
            if(parameters != default)
                Parameters = new ArrayImpl(parameters);
            
            Identifier = new GuidWrapper();
            
            _methIndex = Target.GetMethodNumber(MethodName);
            _method = Target.GetMethodInfo(_methIndex);
        }

        public Task WorkerTask
        {
            get => _workerTask;
            set
            {
                _workerTask = value;
                _taskId = _workerTask.Id;
            }
        }

        public int TaskId => _taskId;

        /// <summary>
        /// Задание, из которого запущено это, или null, если запущено не из фонового задания
        /// </summary>
        internal BackgroundTask Parent { get; set; }

        internal bool IsDescendantOf(BackgroundTask ancestor)
        {
            for (var task = Parent; task != null; task = task.Parent)
            {
                if (ReferenceEquals(task, ancestor))
                    return true;
            }

            return false;
        }

        public CancellationToken CancellationToken => _cancellation.Token;

        [ContextProperty("УникальныйИдентификатор","UUID")]
        public GuidWrapper Identifier { get; private set; }
        
        [ContextProperty("ИмяМетода","MethodName")]
        public string MethodName { get; private set; }
        
        [ContextProperty("Объект","Object")]
        public IRuntimeContextInstance Target { get; private set; }

        [ContextProperty("Состояние", "State")]
        public TaskStateEnum State { get; private set; }

        [ContextProperty("Параметры", "Parameters")]
        public IValue Parameters { get; private set; } = ValueFactory.Create();

        [ContextProperty("Результат", "Result")]
        public IValue Result { get; private set; } = ValueFactory.Create();

        [ContextProperty("ИнформацияОбОшибке", "ExceptionInfo")]
        public ExceptionInfoContext ExceptionInfo { get; private set; }

        /// <summary>
        /// Ждать завершения задания указанное число миллисекунд
        /// </summary>
        /// <param name="process">Текущий процесс, в котором вызван данный метод</param>
        /// <param name="timeout">Таймаут в миллисекундах. Если ноль - ждать вечно</param>
        /// <returns>Истина - дождались завершения. Ложь - сработал таймаут</returns>
        [ContextMethod("ОжидатьЗавершения", "Wait")]
        public bool Wait(IBslProcess process, int timeout = 0)
        {
            timeout = BackgroundTasksManager.ConvertTimeout(timeout);
            
            return WorkerTask.Wait(timeout, process.CancellationToken);
        }
        
        /// <summary>
        /// Отменяет выполнение задания. Код задания прерывается перед выполнением очередной строки
        /// или во время Приостановить(), перехватить отмену через Попытка нельзя.
        /// Обработчики завершения потока исполнения при этом отрабатывают.
        /// Метод не дожидается остановки задания, для этого используйте ОжидатьЗавершения().
        /// Отмена завершенного задания ничего не делает.
        /// </summary>
        [ContextMethod("Отменить", "Cancel")]
        public void Cancel()
        {
            lock (_cancellationLock)
            {
                // Источник отмены освобождается, когда задание завершилось
                if (!_cancellationReleased)
                    _cancellation.Cancel();
            }
        }

        public void ExecuteOnCurrentThread(IBslProcess process)
        {
            try
            {
                Execute(process);
            }
            finally
            {
                ReleaseCancellation();
            }
        }

        // Ожидание в Приостановить() создает у источника отмены событие ядра,
        // а задание остается в списке менеджера до Очистить()
        private void ReleaseCancellation()
        {
            lock (_cancellationLock)
            {
                _cancellationReleased = true;
                _cancellation.Dispose();
            }
        }

        private void Execute(IBslProcess process)
        {
            if (State != TaskStateEnum.NotRunned)
                throw new RuntimeException(Locale.NStr("ru = 'Неверное состояние задачи';en = 'Incorrect task status'"));

            if (_cancellation.IsCancellationRequested)
            {
                State = TaskStateEnum.Canceled;
                return;
            }

            var parameters = Parameters is ArrayImpl array ?
                array.ToArray() : Array.Empty<IValue>();

            try
            {
                State = TaskStateEnum.Running;
                if (_method.IsFunction())
                {
                    Target.CallAsFunction(_methIndex, parameters, out var result, process);
                    Result = result;
                }
                else
                {
                    Target.CallAsProcedure(_methIndex, parameters, process);
                }

                State = TaskStateEnum.Completed;
            }
            catch (ScriptException exception)
            {
                State = TaskStateEnum.CompletedWithErrors;
                exception.RuntimeSpecificInfo = process.Services
                    .TryResolve<StackMachineProvider>()?.Machine?.GetExecutionFrames();
                
                ExceptionInfo = new ExceptionInfoContext(exception);
            }
            catch (OperationCanceledException exception) when (exception.CancellationToken == _cancellation.Token)
            {
                State = TaskStateEnum.Canceled;
            }
            catch (Exception exception)
            {
                // Метод встроенного объекта бросает исключения .NET, а не ScriptException:
                // без этого задание осталось бы «Активно» без информации об ошибке
                State = TaskStateEnum.CompletedWithErrors;
                ExceptionInfo = new ExceptionInfoContext(new ExternalSystemException(exception));
            }
        }
    }
}
