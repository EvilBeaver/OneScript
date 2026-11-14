/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol.TcpServer
{
    public sealed class JsonDtoChannel : IMessageChannel
    {
        private readonly IDebuggerClient _client;
        private readonly Stream _dataStream;
        
        // Пишут и поток сообщений (ответы), и потоки скриптов (события остановки)
        private readonly object _writeLock = new object();
        private volatile bool _enabled = true;

        public JsonDtoChannel(IDebuggerClient client)
        {
            _client = client;
            _dataStream = client.GetDataStream();
        }
        
        public JsonDtoChannel(Stream dataStream)
        {
            _client = null;
            _dataStream = dataStream;
        }

        public void Dispose()
        {
            // Без блокировки записи: закрытие потока как раз прерывает зависшую запись
            _enabled = false;
            _dataStream.Dispose();
            _client?.Dispose();
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
                    // Сообщение целиком, чтобы сообщения разных потоков не перемешались
                    lock (_writeLock)
                    {
                        bufferedStream.CopyTo(_dataStream);
                    }
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

            try
            {
                using (var socketReader = new BinaryReader(_dataStream, Encoding.UTF8, true))
                {
                    var contentLength = socketReader.ReadInt32();
                    var contentBuffer = new byte[contentLength];
                    StreamUtils.ReadStream(socketReader.BaseStream, contentBuffer, contentLength);

                    using (var textReader = new StreamReader(new MemoryStream(contentBuffer), Encoding.UTF8, false))
                    {
                        using var reader = new JsonTextReader(textReader);
                        return JsonSerializer.CreateDefault().Deserialize<TcpProtocolDtoBase>(reader);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException)
            {
                // Соединение закрыто или оборвалось: дальше читать нечего
                throw new ChannelException("Channel is closed", true, ex);
            }
            catch (Exception ex)
            {
                throw new ChannelException("Channel read exception", ex);
            }
        }
        
        public bool Connected => _enabled && (_client?.Connected ?? true);
    }
}