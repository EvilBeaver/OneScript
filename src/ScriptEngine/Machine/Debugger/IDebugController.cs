/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine.Debugger;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Управляет возможностью отладки на уровне всего хост-процесса.
    /// </summary>
    public interface IDebugController : IDisposable
    {
        /// <summary>
        /// Начинает слушать входящие подключения со стороны IDE.
        /// </summary>
        void ListenConnections();
        
        /// <summary>
        /// Блокирует поток до момента подключения от IDE.
        /// </summary>
        void WaitForSession();
        
        /// <summary>
        /// Уведомляет IDE о завершении отлаживаемого процесса.
        /// </summary>
        /// <param name="exitCode">код выхода приложения</param>
        void NotifyProcessExit(int exitCode);
        
        IBreakpointManager BreakpointManager { get; }
        
        IThreadEventsListener ThreadManager { get; }
    }
}
