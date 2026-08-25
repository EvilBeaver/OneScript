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
    /// </summary>
    [ContextClass("ПотокИсполнения", "ExecutionThread")]
    public sealed class ExecutionThreadContext : AutoContext<ExecutionThreadContext>, IDisposable
    {
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

            Threads.Remove(process);
            thread.Dispose();
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
