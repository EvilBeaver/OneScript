/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
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
    public sealed class ExecutionThreadContext : AutoContext<ExecutionThreadContext>, IDisposable
    {
        /// <summary>
        /// Имена события завершения потока исполнения. Событие поднимается под обоими именами,
        /// поэтому подписаться можно как на русское, так и на английское.
        /// </summary>
        private static readonly string[] TerminationEventNames = { "ПриЗавершении", "OnTermination" };

        private static readonly ConditionalWeakTable<IBslProcess, ExecutionThreadContext> Threads = new();

        private ExecutionThreadContext(IBslProcess process)
        {
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
        [ContextProperty("Идентификатор", "Id", CanWrite = false)]
        public int Identifier { get; }

        /// <summary>
        /// Хранилище данных потока исполнения, аналог набора thread-local переменных.
        ///
        /// Соответствие создаётся вместе с потоком исполнения и не разделяется с другими потоками.
        /// В конце потока исполнения соответствие очищается, а его значения, поддерживающие
        /// интерфейс IDisposable среды CLR, принудительно освобождаются.
        /// </summary>
        /// <value>Соответствие. Данные потока исполнения.</value>
        [ContextProperty("Данные", "Data", CanWrite = false)]
        public MapImpl Data { get; } = new MapImpl();

        /// <summary>
        /// Возвращает поток исполнения указанного bsl-процесса, создавая его при первом обращении.
        /// Для одного процесса всегда возвращается один и тот же экземпляр.
        /// </summary>
        internal static ExecutionThreadContext Of(IBslProcess process)
        {
            return Threads.GetValue(process, p => new ExecutionThreadContext(p));
        }

        /// <summary>
        /// Завершает поток исполнения процесса, освобождая его данные.
        ///
        /// Вызывается владельцем процесса, когда процесс отработал: менеджером фоновых заданий
        /// по завершении задания и веб-сервером по окончании обработки запроса. Если поток
        /// исполнения не создавался, метод ничего не делает.
        /// </summary>
        public static void Release(IBslProcess process)
        {
            if (process == null)
                return;

            if (!Threads.TryGetValue(process, out var thread))
                return;

            thread.RaiseTerminationEvent(process);

            Threads.Remove(process);
            thread.Dispose();
        }

        /// <summary>
        /// Поднимает событие завершения потока исполнения.
        ///
        /// Событие поднимается до очистки данных, поэтому обработчик ещё видит всё, что поток
        /// в них положил, и может, например, вернуть занятые ресурсы владельцу.
        ///
        /// Ошибка обработчика не выпускается наружу: поток завершается уже после того, как
        /// код единицы исполнения отработал, и ронять на этом её результат нельзя. У фонового
        /// задания завершение идёт в блоке finally и затёрло бы исходную ошибку, у веб-сервера
        /// оно выполняется после отправки ответа.
        /// </summary>
        private void RaiseTerminationEvent(IBslProcess process)
        {
            var eventProcessor = process.Services.TryResolve<IEventProcessor>();
            if (eventProcessor == null)
                return;

            try
            {
                foreach (var eventName in TerminationEventNames)
                {
                    try
                    {
                        eventProcessor.HandleEvent(this, eventName, Array.Empty<IValue>(), process);
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
                // Реестр подписок держит источник до конца работы движка, а поток исполнения
                // живёт лишь до конца своей единицы исполнения. Без снятия подписок каждый
                // завершившийся поток оставался бы в реестре навсегда.
                eventProcessor.RemoveAllHandlers(this);
            }
        }

        public void Dispose()
        {
            foreach (var item in Data)
            {
                (item.Value as IDisposable)?.Dispose();
            }

            Data.Clear();
        }
    }
}
