/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace oscript.DebugServer
{
    public class TcpChannel : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _clientStream;
        private readonly BinaryFormatter _serializer;

        public TcpChannel(TcpClient client)
        {
            _client = client;
            _clientStream = _client.GetStream();
            _serializer = new BinaryFormatter();
        }

        public bool Connected => _client.Connected;
        
        public void Write(object data)
        {
            _serializer.Serialize(_clientStream, data);
        }
        
        public T Read<T>()
        {
            return (T)_serializer.Deserialize(_clientStream);
        }

        public void Dispose()
        {
            _clientStream.Dispose();
            _client.Close();
        }
    }
}