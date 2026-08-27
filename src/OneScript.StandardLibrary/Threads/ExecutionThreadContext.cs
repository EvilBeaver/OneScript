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

namespace OneScript.StandardLibrary.Threads
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
    public sealed class ExecutionThreadContext : AutoContext<ExecutionThreadContext>, IBslExecutionThread
    {
        /// <summary>
        /// Имена события завершения потока исполнения. Событие поднимается под обоими именами,
        /// поэтому подписаться можно как на русское, так и на английское.
        /// </summary>
        private static readonly string[] TerminationEventNames = { "ПриЗавершении", "OnTermination" };

        private readonly IBslProcess _process;

        private bool _terminated;

        private ExecutionThreadContext(IBslProcess process)
        {
            _process = process;
            Identifier = process.VirtualThreadId;
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
        public int Identifier { get; }

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
            if (process.ExecutionThread is ExecutionThreadContext existing)
                return existing;

            lock (process)
            {
                if (process.ExecutionThread is ExecutionThreadContext created)
                    return created;

                var thread = new ExecutionThreadContext(process);
                process.ExecutionThread = thread;

                return thread;
            }
        }

        /// <summary>
        /// Оповещает подписчиков о завершении потока и снимает их подписки.
        ///
        /// Событие поднимается до очистки данных: обработчик ещё видит всё, что поток успел в них
        /// положить. Именно так пул соединений забирает обратно соединение, которое отработавший
        /// код не освободил сам.
        ///
        /// Ошибка обработчика наружу не выходит, только в лог. К этому моменту код единицы
        /// исполнения уже отработал: у фонового задания завершение идёт в блоке finally и
        /// затёрло бы исходную ошибку, у веб-сервера - после отправки ответа.
        /// </summary>
        private void RaiseTerminationEvent()
        {
            var eventProcessor = _process.Services.TryResolve<IEventProcessor>();
            if (eventProcessor == null)
                return;

            try
            {
                foreach (var eventName in TerminationEventNames)
                {
                    try
                    {
                        eventProcessor.HandleEvent(this, eventName, Array.Empty<IValue>(), _process);
                    }
                    catch (Exception exception)
                    {
                        SystemLogger.Write(
                            $"WARNING! Error in execution thread termination handler '{eventName}': {exception.Message}");
                    }
                }
            }
            finally
            {
                // Процессор событий держит источник, пока подписки не сняты. Потоков исполнения
                // много и живут они недолго, поэтому без явного снятия реестр рос бы бесконечно.
                eventProcessor.RemoveAllHandlers(this);
            }
        }

        /// <summary>
        /// Завершает поток исполнения: оповещает подписчиков и освобождает данные.
        ///
        /// Вызывается процессом, когда тот освобождается.
        ///
        /// Каждое значение освобождается отдельно, ошибка на одном не мешает остальным и наружу
        /// не выходит. Значения перебираются по копии: освобождаемое значение вправе изменить
        /// эти же данные, а перебор живой карты сорвался бы на следующем шаге - причём мимо
        /// защиты, которой окружено само освобождение.
        /// </summary>
        public void Terminate()
        {
            // Пока идёт завершение, поток ещё числится за процессом, и обработчик вправе
            // добраться до него через ТекущийПоток(). Если он при этом освободит процесс,
            // завершение не должно пойти по второму кругу.
            if (_terminated)
                return;

            _terminated = true;

            RaiseTerminationEvent();

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
