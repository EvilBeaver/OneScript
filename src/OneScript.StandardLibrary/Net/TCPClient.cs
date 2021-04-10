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
using OneScript.StandardLibrary.Binary;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Net
{
    /// <summary>
    /// Соединение по протоколу TCP. Позволяет отправлять и принимать данные с использованием TCP сокета.
    /// </summary>
    [ContextClass("TCPСоединение","TCPConnection")]
    public class TCPClient : AutoContext<TCPClient>, IDisposable
    {
        private readonly TcpClient _client;

        public TCPClient(TcpClient client)
        {
            this._client = client;
        }

        /// <summary>
        /// Прочитать данные из сокета в виде строки.
        /// </summary>
        /// <param name="encoding">КодировкаТекста или Строка. Указывает в какой кодировке интерпретировать входящий поток байт.
        /// Значение по умолчанию: utf-8</param>
        /// <returns>Строка. Данные прочитанные из сокета</returns>
        [ContextMethod("ПрочитатьСтроку","ReadString")]
        public string ReadString(string encoding = null)
        {
            const int NO_LIMIT = 0;
            var memStream = ReadAllData(_client.GetStream(), NO_LIMIT);
            var enc = GetEncodingByName(encoding);
            if (memStream.Length == 0)
                return "";

            using(var reader = new StreamReader(memStream, enc))
            {
                return reader.ReadToEnd();
            }

        }

        /// <summary>
        /// Читает сырые байты из сокета.
        /// </summary>
        /// <param name="len">Количество байт, которые требуется прочитать. 0 - читать до конца потока.
        /// Значение по умолчанию: 0</param>
        /// <returns>ДвоичныеДанные</returns>
        [ContextMethod("ПрочитатьДвоичныеДанные", "ReadBinaryData")]
        public BinaryDataContext ReadBinaryData(int len = 0)
        {
            var stream = _client.GetStream();
            var ms = ReadAllData(stream, len);
            var data = ms.ToArray();

            return new BinaryDataContext(data);
        }

        private MemoryStream ReadAllData(NetworkStream source, int limit)
        {
            const int BUF_SIZE = 1024;
            byte[] readBuffer = new byte[BUF_SIZE];

            bool useLimit = limit > 0;
            var ms = new MemoryStream();

            do
            {
                int portion = useLimit ? Math.Min(limit, BUF_SIZE) : BUF_SIZE;
                int numberOfBytesRead = source.Read(readBuffer, 0, portion);
                ms.Write(readBuffer, 0, numberOfBytesRead);
                if (useLimit)
                    limit -= numberOfBytesRead;
            } while (source.DataAvailable);
            
            if(ms.Length > 0)
                ms.Position = 0;

            return ms;

        }

        /// <summary>
        /// Отправка строки на удаленный хост
        /// </summary>
        /// <param name="data">Строка. Данные для отправки</param>
        /// <param name="encoding">КодировкаТекста или Строка. Кодировка в которой нужно записать данные в поток. По умолчанию utf-8</param>
        [ContextMethod("ОтправитьСтроку","SendString")]
        public void SendString(string data, string encoding = null)
        {
            if(data == String.Empty)
                return;

            var enc = GetEncodingByName(encoding);
            byte[] bytes = enc.GetBytes(data);
            var stream = _client.GetStream();
            
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        /// <summary>
        /// Отправка сырых двоичных данных на удаленный хост.
        /// </summary>
        /// <param name="data">ДвоичныеДанные которые нужно отправить.</param>
        [ContextMethod("ОтправитьДвоичныеДанные", "SendBinaryData")]
        public void SendString(BinaryDataContext data)
        {
            if (data.Buffer.Length == 0)
                return;

            var stream = _client.GetStream();
            stream.Write(data.Buffer, 0, data.Buffer.Length);
            stream.Flush();

        }

        /// <summary>
        /// Признак активности соединения.
        /// Данный признак не является надежным признаком существования соединения. 
        /// Он говорит лишь о том, что на момент получения значения данного свойства соединение было активно.
        /// </summary>
        [ContextProperty("Активно","IsActive")]
        public bool IsActive
        {
            get 
            {
                const int POLL_INTERVAL = 500;
                var socket = _client.Client;
                
                return !((socket.Poll(POLL_INTERVAL, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
            }
        }

        /// <summary>
        /// Таймаут, в течение которого система ожидает отправки данных. Если таймаут не установлен, то скрипт будет ждать начала отправки бесконечно.
        /// </summary>
        [ContextProperty("ТаймаутОтправки", "WriteTimeout")]
        public int WriteTimeout
        {
            get { return _client.GetStream().WriteTimeout; }
            set { _client.GetStream().WriteTimeout = value; }
        }

        /// <summary>
        /// Таймаут чтения данных. Если таймаут не установлен, то скрипт будет ждать начала приема данных бесконечно.
        /// </summary>
        [ContextProperty("ТаймаутЧтения", "ReadTimeout")]
        public int ReadTimeout
        {
            get { return _client.GetStream().ReadTimeout; }
            set { _client.GetStream().ReadTimeout = value; }
        }

        private static Encoding GetEncodingByName(string encoding)
        {
            Encoding enc;
            if (encoding == null)
            {
                enc = new UTF8Encoding();
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
            }

            return enc;
        }

        /// <summary>
        /// Закрывает соединение с удаленным хостом.
        /// </summary>
        [ContextMethod("Закрыть","Close")]
        public void Close()
        {
            _client.Close();
        }

        /// <summary>
        /// Возвращает адрес удаленного узла соединения
        /// </summary>
        [ContextProperty("УдаленныйУзел", "RemoteEndPoint")]
        public string RemoteEndPoint => _client.Client.RemoteEndPoint.ToString();


        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Подключение к удаленному TCP-сокету
        /// </summary>
        /// <param name="host">адрес машины</param>
        /// <param name="port">порт сокета</param>
        [ScriptConstructor]
        public static TCPClient Constructor(IValue host, IValue port)
        {
            var client = new TcpClient(host.AsString(), (int)port.AsNumber());
            return new TCPClient(client);
        }
    }
}
