/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.DebugProtocol
{
    /// <summary>
    /// Интерфейс слушателя событий отладки (сообщений, инициируемых со стороны BSL)       
    /// </summary>
    public interface IDebugEventListener
    {
        void ThreadStopped(int threadId, ThreadStopReason reason);
        
        void ThreadStoppedEx(int threadId, ThreadStopReason reason, string errorMessage);

        void ProcessExited(int exitCode);
    }
}