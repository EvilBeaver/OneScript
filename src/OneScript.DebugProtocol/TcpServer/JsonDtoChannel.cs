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
using Newtonsoft.Json;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol.TcpServer
{
    public class JsonDtoChannel : ICommunicationChannel
    {
        private readonly TcpClient _tcpClient;
        private readonly Stream _dataStream;
        
        private bool _enabled = true;

        public JsonDtoChannel(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _dataStream = tcpClient.GetStream();
        }
        
        public JsonDtoChannel(Stream dataStream)
        {
            _tcpClient = null;
            _dataStream = dataStream;
        }

        public void Dispose()
        {
            _dataStream.Dispose();
            _tcpClient?.Close();
            _enabled = false;
        }

        public void Write(object data)
        {
            if (!_enabled)
                throw new ObjectDisposedException(nameof(JsonDtoChannel));
            
            var content = JsonConvert.SerializeObject(data);
            var contentBytes = Encoding.UTF8.GetBytes(content);

            using (var bufferedStream = new MemoryStream(contentBytes.Length + sizeof(int)))
            {
                using (var writer = new BinaryWriter(bufferedStream, Encoding.UTF8))
                {
                    writer.Write(contentBytes.Length);
                    writer.Write(contentBytes, 0, contentBytes.Length);

                    bufferedStream.Position = 0;
                    bufferedStream.CopyTo(_dataStream);
                }
            }
        }
        
        public T Read<T>()
        {
            return (T)Read();
        }

        public object Read()
        {
            if (!_enabled)
                throw new ObjectDisposedException(nameof(JsonDtoChannel));

            using (var socketReader = new BinaryReader(_dataStream, Encoding.UTF8, true))
            {
                var contentLength = socketReader.ReadInt32();
                var contentBuffer = new byte[contentLength];
                ReadStream(socketReader, contentBuffer, contentLength);

                using (var textReader = new StreamReader(new MemoryStream(contentBuffer), Encoding.UTF8, false))
                {
                    using var reader = new JsonTextReader(textReader);
                    return JsonSerializer.CreateDefault().Deserialize<TcpProtocolDtoBase>(reader);
                }
            }
        }

        private void ReadStream(BinaryReader reader, byte[] buffer, int length)
        {
            int readPosition = 0;
            int bytesReceived = 0;

            while (bytesReceived < length)
            {
                bytesReceived = reader.Read(buffer, readPosition, length - bytesReceived);
                if (bytesReceived == 0)
                    throw new IOException("Unexpected end of stream");
                
                readPosition += bytesReceived;
            }
        }
        
        public bool Connected => _enabled && (_tcpClient?.Connected ?? true);
    }
}