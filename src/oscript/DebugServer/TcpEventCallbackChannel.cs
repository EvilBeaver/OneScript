/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using OneScript.DebugProtocol;

namespace oscript.DebugServer
{
    public class TcpEventCallbackChannel : IDebugEventListener, IDisposable
    {
        private readonly TcpChannel _channel;

        public TcpEventCallbackChannel(TcpChannel channel)
        {
            _channel = channel;
        }

        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessExited(int exitCode)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}