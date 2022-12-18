/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol
{
    public class BinaryChannel : ICommunicationChannel
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _clientStream;
        private readonly BinaryFormatter _serializer;

        public BinaryChannel(TcpClient client)
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
            return (T)Read();
        }

        public object Read()
        {
            try
            {
                return _serializer.Deserialize(_clientStream);
            }
            catch (SerializationException e)
            {
                throw new ChannelException("Serialization Exception occured", !Connected, e);
            }
        }

        public void Dispose()
        {
            _clientStream.Dispose();
            _client.Close();
        }
    }
}