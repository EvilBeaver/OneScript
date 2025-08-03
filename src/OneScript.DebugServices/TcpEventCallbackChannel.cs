/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
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

        public void ThreadStoppedEx(int threadId, ThreadStopReason reason, string errorMessage)
        {
            var dto = RpcCall.Create(nameof(IDebugEventListener), nameof(ThreadStoppedEx), threadId, reason, errorMessage);
            Write(dto);
        }
        
        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            var dto = RpcCall.Create(nameof(IDebugEventListener), nameof(ThreadStopped), threadId, reason);
            Write(dto);
        }

        public void ProcessExited(int exitCode)
        {
            var dto = RpcCall.Create(nameof(IDebugEventListener), nameof(ProcessExited), exitCode); 
            Write(dto);
        }

        private void Write(RpcCall dto)
        {
            if (!_channel.Connected)
                return;

            try
            {
                _channel.Write(dto);
            }
            catch (IOException)
            {
                // Ignore
            }
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}