/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#nullable enable

using System;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Execution
{
    /// <summary>
    /// Готовый к исполнению bsl-процесс, с настроенным окружением.
    ///
    /// Процесс освобождается тем, кто его создал, когда единица исполнения отработала.
    /// Освобождение завершает поток исполнения процесса.
    /// </summary>
    public interface IBslProcess
    {
        /// <summary>
        /// Запустить метод в текущем процессе
        /// </summary>
        /// <param name="target">целевой объект</param>
        /// <param name="module">модуль bsl-кода, который запускается</param>
        /// <param name="method">bsl-метод, который запускается</param>
        /// <param name="arguments">аргументы метода</param>
        /// <returns>Возвращаемое значение. default если вызывалась процедура</returns>
        public BslValue? Run(BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments); 
            
        /// <summary>
        /// Сервисы текущего процесса
        /// </summary>
        public IServiceContainer Services { get; }

        public int VirtualThreadId { get; }

        /**
         * Обертка текущего процесса в терминах BSL.
         * Может отсутствовать, полностью определяется хост-приложением процесса.
         */
        public IRuntimeContextInstance? BslWrapper
        {
            get => null;
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }
    }
}