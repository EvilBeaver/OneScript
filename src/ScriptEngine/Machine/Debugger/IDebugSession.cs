/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Machine.Debugger
{
    public interface IDebugSession : IDisposable
    {
        /// <summary>
        /// Поставщик точек останова для машины.
        /// </summary>
        IBreakpointManager BreakpointManager { get; }
        
        /// <summary>
        /// Уведомляет о создании или завершении потоков.
        /// </summary>
        IThreadEventsListener ThreadManager { get; }

        /// <summary>
        /// Ожидает явной команды Execute от IDE.
        /// В процессе запуска IDE устанавливает точки останова и присылает команду Execute,
        /// запускающую поток исполнения. К моменту исполнения самой первой строчки кода
        /// все точки останова уже заданы и мы можем ожидать их срабатывания.
        ///
        /// Метод не блокируется, если сессия отладки запущена в режиме attach.
        /// Метод блокируется в т.ч. если сессия отладки не начата вообще.
        /// </summary>
        void WaitReadyToRun();
        
        /// <summary>
        /// Указывает, что сессия отладки активна (инициирована со стороны IDE).
        /// </summary>
        bool IsActive { get; }
    }
}