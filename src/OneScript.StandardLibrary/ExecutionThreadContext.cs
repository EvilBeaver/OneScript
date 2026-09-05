/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Поток исполнения bsl-кода.
    ///
    /// Потоком исполнения является каждая независимая единица исполнения: основной скрипт,
    /// каждое фоновое задание и каждый обрабатываемый запрос веб-сервера. Получить поток
    /// исполнения текущего кода можно функцией ТекущийПоток().
    ///
    /// Свойство Данные представляет собой хранилище, живущее ровно столько же, сколько живёт
    /// сам поток исполнения. Оно предназначено для библиотек, которым нужно хранить состояние
    /// в разрезе единицы исполнения (аналог thread-local хранилища).
    ///
    /// По завершении потока исполнения поднимается событие ПриЗавершении (оно же OnTermination),
    /// на которое можно подписаться штатным способом:
    ///
    ///     ДобавитьОбработчик ТекущийПоток().ПриЗавершении, ЭтотОбъект.ВернутьСоединениеВПул;
    ///
    /// Обработчик вызывается до очистки данных потока, поэтому ещё видит их содержимое.
    /// Это позволяет владельцам ресурсов узнавать о конце единицы исполнения вместо того,
    /// чтобы опрашивать список фоновых заданий.
    /// </summary>
    [ContextClass("ПотокИсполнения", "ExecutionThread")]
    public sealed class ExecutionThreadContext : AutoContext<ExecutionThreadContext>, IDisposable
    {
        private readonly IBslProcess _process;

        private bool _isDisposed;

        private ExecutionThreadContext(IBslProcess process)
        {
            _process = process;
        }

        /// <summary>
        /// Идентификатор потока исполнения. Предназначен для диагностики и журналирования.
        ///
        /// Идентификаторы выдаются последовательно и в пределах запущенного движка не повторяются,
        /// пока счётчик не исчерпает диапазон Int32. Для хранения состояния в разрезе потока
        /// исполнения используйте свойство Данные, а не идентификатор в качестве ключа.
        /// </summary>
        /// <value>Число. Идентификатор потока исполнения.</value>
        [ContextProperty("Идентификатор", "Id")]
        public int Identifier => _process.VirtualThreadId;

        /// <summary>
        /// Хранилище данных потока исполнения, аналог набора thread-local переменных.
        ///
        /// Соответствие создаётся вместе с потоком исполнения и не разделяется с другими потоками.
        /// В конце потока исполнения соответствие очищается, а его значения, поддерживающие
        /// интерфейс IDisposable среды CLR, принудительно освобождаются.
        /// </summary>
        /// <value>Соответствие. Данные потока исполнения.</value>
        [ContextProperty("Данные", "Data")]
        public MapImpl Data { get; } = new MapImpl();

        /// <summary>
        /// Возвращает поток исполнения указанного bsl-процесса, создавая его при первом обращении.
        /// Для одного процесса всегда возвращается один и тот же экземпляр.
        ///
        /// Созданный поток остаётся на процессе и освобождается вместе с ним.
        /// </summary>
        internal static ExecutionThreadContext Of(IBslProcess process)
        {
            if (process.BslWrapper is ExecutionThreadContext wrapper)
                return wrapper;
            
            lock (process)
            {
                if (process.BslWrapper is ExecutionThreadContext created)
                    return created;
                
                if (process.BslWrapper != null)
                    throw new InvalidOperationException($"BslWrapper for process is not {nameof(ExecutionThreadContext)}: {process.BslWrapper.GetType()}");

                var thread = new ExecutionThreadContext(process);
                process.BslWrapper = thread;

                return thread;
            }
        }

        public void Dispose()
        {
            // Пока идёт завершение, поток ещё числится за процессом, и обработчик вправе
            // добраться до него через ТекущийПоток(). Если он при этом освободит процесс,
            // завершение не должно пойти по второму кругу.
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                foreach (var item in Data.ToArray())
                {
                    if (item.Value is not IDisposable disposable)
                        continue;

                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception exception)
                    {
                        SystemLogger.Write(
                            $"WARNING! Error releasing execution thread data '{item.Key}': {exception.Message}");
                    }
                }
            }
            finally
            {
                Data.Clear();
            }
        }
    }
}
