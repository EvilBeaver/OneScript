/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.DebugProtocol.Abstractions
{
    /// <summary>
    /// Сервис прослушивания сетевого канала и отправки в него сообщений 
    /// </summary>
    public interface ICommunicationServer
    {
        void Start();

        void Stop();

        event EventHandler<CommunicationEventArgs> DataReceived;
        event EventHandler<CommunicationEventArgs> OnError;
    }
}