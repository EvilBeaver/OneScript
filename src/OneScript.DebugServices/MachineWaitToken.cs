/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    /// <summary>
    /// Остановка потока машины в отладчике. Поток машины ждет здесь команды отладчика.
    /// </summary>
    public class MachineWaitToken: IDisposable
    {
        private readonly object _lock = new object();
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
        /// Ожидание продолжения в потоке машины
        /// </summary>
        public void Wait()
        {
            lock (_lock)
            {
                while (_stopped)
                    Monitor.Wait(_lock);
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