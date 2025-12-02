/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Debugger
{
    /// <summary>
    /// Менеджер состояния и количества потоков для отладчика
    /// </summary>
    public interface IThreadEventsListener
    {
        void ThreadStarted(int threadId, MachineInstance machine);
        void ThreadStopped(int threadId, MachineStopReason reason, string errorMessage);
        void ThreadExited(int threadId);
    }
}