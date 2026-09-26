/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Threading;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Класс предназначен для блокировки потока для монопольного доступа к ресурсу 
    /// </summary>
    [ContextClass("БлокировкаРесурса", "ResourceLock")]
    public class CriticalSectionContext : AutoContext<CriticalSectionContext>, IDisposable
    {
        // Monitor не принимает токен отмены, поэтому ожидание блокировки идет порциями
        private const int CancellationPollInterval = 50;

        private object _lockObject;
        
        private CriticalSectionContext()
        {
            _lockObject = new object();
        }

        private CriticalSectionContext(object lockObject)
        {
            _lockObject = lockObject;
        }

        /// <summary>
        /// Захватывает блокировку, ожидая ее освобождения другими потоками.
        /// Ожидание прерывается, если фоновое задание, в котором оно выполняется, отменено.
        /// Блокировки, не отпущенные до завершения задания, освобождаются автоматически.
        /// </summary>
        /// <param name="timeout">Таймаут ожидания в миллисекундах. 0 - ждать бесконечно</param>
        /// <returns>Истина - блокировка захвачена, Ложь - истек таймаут</returns>
        [ContextMethod("Заблокировать", "Lock")]
        public bool Lock(IBslProcess process, int timeout = 0)
        {
            if (timeout < 0)
                throw RuntimeException.InvalidArgumentValue();

            var entered = TryEnter(timeout == 0 ? Timeout.Infinite : timeout, process.CancellationToken);
            if (entered)
                (process as BslProcess)?.ResourceLocks.Entered(_lockObject);

            return entered;
        }

        private bool TryEnter(int timeout, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return Monitor.TryEnter(_lockObject, timeout);

            if (Monitor.TryEnter(_lockObject))
                return true;

            var start = Stopwatch.GetTimestamp();
            while (true)
            {
                var wait = timeout == Timeout.Infinite
                    ? CancellationPollInterval
                    : (int)Math.Clamp(timeout - Stopwatch.GetElapsedTime(start).TotalMilliseconds, 0, CancellationPollInterval);

                if (Monitor.TryEnter(_lockObject, wait))
                    return true;

                cancellationToken.ThrowIfCancellationRequested();

                if (timeout != Timeout.Infinite && Stopwatch.GetElapsedTime(start).TotalMilliseconds >= timeout)
                    return false;
            }
        }
        
        [ContextMethod("Разблокировать", "Unlock")]
        public void Unlock()
        {
            Monitor.Exit(_lockObject);
        }
        
        public void Dispose()
        {
            if(Monitor.IsEntered(_lockObject))
                Monitor.Exit(_lockObject);
            
            _lockObject = null;
        }

        [ScriptConstructor]
        public static CriticalSectionContext Create()
        {
            return new CriticalSectionContext();
        }
        
        [ScriptConstructor(Name = "По объекту")]
        public static CriticalSectionContext Create(IValue instance)
        {
            return new CriticalSectionContext(instance.AsObject());
        }
    }
}