/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Debugger
{
    /// <summary>
    /// Основной фасад функциональности отладки.
    /// Управляет стартом и остановкой отладки.
    /// </summary>
    public interface IDebugger
    {
        /// <summary>
        /// Возвращает true, если отладка в процессе возможна.
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Запускает отладчик. С этого момента к отладчику можно подключиться.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Получает текущую сессию отладки.
        /// </summary>
        IDebugSession GetSession();
        
        /// <summary>
        /// Уведомляет IDE о завершении отлаживаемого процесса.
        /// </summary>
        /// <param name="exitCode">код выхода приложения</param>
        void NotifyProcessExit(int exitCode);
    }
}