/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;

namespace OneScript.DebugServices.Internal
{
    internal class DelayedConnectionChannel : ICommunicationChannel
    {
        private TcpListener _listener;
        private ICommunicationChannel _connectedChannel;

        // NB! должен быть согласован с перечислением TransportProtocols в адаптере
        private const short JSON_FORMAT_MARKER = 2;
        // NB! должен быть согласован с файлом ProtocolVersions в адаптере
        private const short SUPPORTED_FORMAT_VERSION = 3;
        
        private bool _reconciled;

        public DelayedConnectionChannel(TcpListener listener)
        {
            _listener = listener;
        }

        public void Dispose()
        {
            _listener?.Stop();
            _listener = null;
            _connectedChannel?.Dispose();
        }

        public void Write(object data)
        {
            if(_connectedChannel == null)
                throw new InvalidOperationException("No client connected");
            
            _connectedChannel.Write(data);
        }

        public T Read<T>()
        {
            ReconcileFormat();
            return _connectedChannel.Read<T>();
        }

        public object Read()
        {
            ReconcileFormat();
            return _connectedChannel.Read();
        }

        private void ReconcileFormat()
        {
            if (_reconciled) 
                return;
            
            _listener.Start();
            var tcpClient = _listener.AcceptTcpClient();
            _listener.Stop();
            _listener = null;
            
            var tcpStream = tcpClient.GetStream();

            if (FormatReconcileUtils.CheckReconcileRequest(tcpStream))
            {
                // Да, это наш фейковый заголовок
                FormatReconcileUtils.WriteReconcileResponse(tcpStream, JSON_FORMAT_MARKER, SUPPORTED_FORMAT_VERSION);
            }
            _reconciled = true;
            _connectedChannel = new JsonDtoChannel(tcpClient);
        }

        public bool Connected => _connectedChannel?.Connected ?? false;
    }
}