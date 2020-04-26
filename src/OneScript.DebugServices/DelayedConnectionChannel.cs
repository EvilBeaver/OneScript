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

namespace OneScript.DebugServices
{
    public class DelayedConnectionChannel : ICommunicationChannel
    {
        private TcpListener _listener;
        private BinaryChannel _connectedChannel;

        public DelayedConnectionChannel(TcpListener listener)
        {
            _listener = listener;
        }

        public void Dispose()
        {
            _listener?.Stop();
            _listener = null;
            _connectedChannel.Dispose();
        }

        public void Write(object data)
        {
            if(_connectedChannel == null)
                throw new InvalidOperationException("No client connected");
            
            _connectedChannel.Write(data);
        }

        private void MakeChannel()
        {
            if (_connectedChannel == null)
            {
                _listener.Start();
                var tcpClient = _listener.AcceptTcpClient();
                _connectedChannel = new BinaryChannel(tcpClient);
                _listener.Stop();
                _listener = null;
            }
        }
        
        
        public T Read<T>()
        {
            MakeChannel();
            return _connectedChannel.Read<T>();
        }

        public object Read()
        {
            MakeChannel();
            return _connectedChannel.Read();
        }
    }
}