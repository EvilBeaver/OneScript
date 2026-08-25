/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Execution;
using OneScript.StandardLibrary.Threads;

namespace OneScript.Web.Server
{
    /// <summary>
    /// Scoped-сервис, хранящий bsl-процесс, который обслуживает текущий запрос.
    ///
    /// Один запрос всегда обслуживается одним процессом, поэтому весь bsl-код запроса,
    /// включая обработчик исключений, видит один и тот же ИдентификаторПотокаИсполнения.
    /// Процесс создаётся при первом обращении: запросы, не дошедшие до bsl-кода,
    /// процесс не создают.
    ///
    /// Процесс намеренно не хранится в HttpContext.Items: Items доступны из bsl-кода
    /// как Контекст.Данные и остаются полностью прикладными.
    ///
    /// Область сервисов запроса освобождается вместе с запросом, поэтому здесь же
    /// заканчивается поток исполнения запроса и освобождаются его данные.
    /// </summary>
    internal sealed class RequestBslProcess : IDisposable
    {
        private readonly IBslProcessFactory _processFactory;
        private readonly object _lock = new object();

        private IBslProcess _process;

        public RequestBslProcess(IBslProcessFactory processFactory)
        {
            _processFactory = processFactory;
        }

        public IBslProcess Process
        {
            get
            {
                if (_process != null)
                    return _process;

                lock (_lock)
                {
                    return _process ??= _processFactory.NewProcess();
                }
            }
        }

        public void Dispose()
        {
            // Процесс создаётся лениво, поэтому ради освобождения его создавать не нужно
            ExecutionThreadContext.Release(_process);
        }
    }
}
