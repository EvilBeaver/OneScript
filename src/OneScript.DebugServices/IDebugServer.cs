/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugServices
{
    /// <summary>
    /// Сервер отладки, слушающий соединения от IDE и уведомляющий о подключении нового клиента.
    /// </summary>
    public interface IDebugServer
    {
        void Listen();
        void Stop();
        
        event EventHandler<IDebuggerClient> OnClientConnected;

        event EventHandler<ListenerErrorEventArgs> OnListenException;
    }
}