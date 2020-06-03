/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;

namespace OneScript.DebugServices
{
    public class TcpEventCallbackChannel : IDebugEventListener, IDisposable
    {
        private readonly ICommunicationChannel _channel;

        public TcpEventCallbackChannel(ICommunicationChannel channel)
        {
            _channel = channel;
        }

        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            var dto = RpcCall.Create(nameof(IDebugEventListener), nameof(ThreadStopped), threadId, reason);
            _channel.Write(dto);
        }

        public void ProcessExited(int exitCode)
        {
            var dto = RpcCall.Create(nameof(IDebugEventListener), nameof(ProcessExited), exitCode);
            _channel.Write(dto);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}