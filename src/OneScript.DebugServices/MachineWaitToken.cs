/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    /// <summary>
    /// Остановка потока машины в отладчике. Поток машины ждет здесь команды отладчика,
    /// а отладчик может выполнить в остановленном потоке свое вычисление.
    /// </summary>
    public class MachineWaitToken: IDisposable
    {
        private readonly object _lock = new object();
        private readonly Queue<Action> _debuggerWork = new Queue<Action>();
        private bool _stopped;
        private bool _released;

        public MachineInstance Machine { get; set; }

        /// <summary>
        /// Поток машины остановлен и ждет команды отладчика
        /// </summary>
        public bool IsStopped
        {
            get
            {
                lock (_lock)
                {
                    return _stopped;
                }
            }
        }

        /// <summary>
        /// Поток машины сейчас выполняет вычисление отладчика. Только для потока машины.
        /// </summary>
        public bool IsRunningDebuggerWork { get; private set; }

        /// <summary>
        /// Поток машины останавливается. Вызывается до отправки события остановки,
        /// чтобы команда продолжения, пришедшая сразу за событием, не потерялась.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                // Отладчик уже отключился - больше не останавливаемся
                if (!_released)
                    _stopped = true;
            }
        }

        /// <summary>
        /// Ожидание продолжения в потоке машины. Пока ждет, выполняет вычисления отладчика.
        /// </summary>
        public void Wait()
        {
            while (true)
            {
                Action work;
                lock (_lock)
                {
                    while (_stopped && _debuggerWork.Count == 0)
                        Monitor.Wait(_lock);

                    if (_debuggerWork.Count == 0)
                        return;

                    work = _debuggerWork.Dequeue();
                }

                IsRunningDebuggerWork = true;
                try
                {
                    work();
                }
                finally
                {
                    IsRunningDebuggerWork = false;
                }
            }
        }

        /// <summary>
        /// Продолжить выполнение потока машины
        /// </summary>
        public void Set()
        {
            lock (_lock)
            {
                _stopped = false;
                Monitor.PulseAll(_lock);
            }
        }
        
        /// <summary>
        /// Выполняет вычисление в остановленном потоке машины и ждет результата.
        /// Так вычисление видит блокировки и состояние этого потока, а не потока отладчика.
        /// </summary>
        public T RunOnStoppedThread<T>(Func<T> func)
        {
            T result = default;
            ExceptionDispatchInfo error = null;
            using var completed = new ManualResetEventSlim();

            lock (_lock)
            {
                if (!_stopped)
                    throw new InvalidOperationException("Thread is running");

                _debuggerWork.Enqueue(() =>
                {
                    try
                    {
                        result = func();
                    }
                    catch (Exception e)
                    {
                        error = ExceptionDispatchInfo.Capture(e);
                    }
                    finally
                    {
                        completed.Set();
                    }
                });
                Monitor.PulseAll(_lock);
            }

            completed.Wait();
            error?.Throw();
            return result;
        }
        
        /// <summary>
        /// Отладчик отключается: поток продолжает работу и больше не останавливается
        /// </summary>
        public void Dispose()
        {
            Machine?.UnsetDebugMode();
            lock (_lock)
            {
                _released = true;
                _stopped = false;
                Monitor.PulseAll(_lock);
            }
        }
    }
}