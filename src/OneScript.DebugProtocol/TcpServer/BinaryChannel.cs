/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol
{
    /// <summary>
    /// TCP-канал, использующий стандартную Binary-сериализацию .NET
    /// </summary>
    [Obsolete("Используется только со стороны адаптера, для работы со старыми версиями 1Script")]
    public sealed class BinaryChannel : ICommunicationChannel
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _clientStream;
#if NET48
        private readonly BinaryFormatter _serializer;
#else
        private readonly IFormatter _serializer = null;
#endif

        private bool _enabled;

        public BinaryChannel(TcpClient client)
        {
            _client = client;
            _clientStream = _client.GetStream();
            _enabled = true;
#if NET48
            _serializer = new BinaryFormatter();
#else
            throw new NotSupportedException("Binary channel should be used only in .NET 48.");
#endif
        }

        public bool Connected => _enabled && _client.Connected;
        
        public void Write(object data)
        {
            if (!_enabled)
                throw new ObjectDisposedException(nameof(BinaryChannel));
            
            _serializer.Serialize(_clientStream, data);
        }
        
        public T Read<T>()
        {
            return (T)Read();
        }

        public object Read()
        {
            if (!_enabled)
                throw new ObjectDisposedException(nameof(BinaryChannel));
            
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
            _enabled = false;
        }
    }
}