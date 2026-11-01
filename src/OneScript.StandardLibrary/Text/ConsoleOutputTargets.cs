/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.StandardLibrary.Text
{
    /// <summary>
    /// Куда текущий поток выполнения выводит консоль (Консоль.УстановитьПотокВывода/УстановитьПотокОшибок).
    /// Фоновые задания наследуют это сами, а там, куда контекст выполнения не передается
    /// (обработчики запросов веб-сервера), цели нужно перенести явно.
    /// </summary>
    public sealed class ConsoleOutputTargets
    {
        private readonly (object Out, object Error) _targets;

        private ConsoleOutputTargets((object Out, object Error) targets)
        {
            _targets = targets;
        }

        /// <summary>
        /// Запоминает цели вывода текущего потока выполнения
        /// </summary>
        public static ConsoleOutputTargets Capture() => new ConsoleOutputTargets(ConsoleWriterRouter.CaptureTargets());

        /// <summary>
        /// Устанавливает запомненные цели в текущем потоке выполнения.
        /// Вызванный внутри async-метода, действует только до его завершения.
        /// </summary>
        public void Apply() => ConsoleWriterRouter.ApplyTargets(_targets);
    }
}
