using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.IO;

namespace ScriptEngine.HostedScript.Library.Net
{
    [ContextClass("TCPСоединение","TCPConnection")]
    public class TCPClient : AutoContext<TCPClient>, IDisposable
    {
        private TcpClient _client;

        public TCPClient(TcpClient client)
        {
            this._client = client;
        }

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

            while (source.DataAvailable)
            {
                int portion = useLimit ? Math.Min(limit, BUF_SIZE) : BUF_SIZE;
                int numberOfBytesRead = source.Read(readBuffer, 0, portion);
                ms.Write(readBuffer, 0, numberOfBytesRead);
                if (useLimit)
                    limit -= numberOfBytesRead;
            }
            
            if(ms.Length > 0)
                ms.Position = 0;

            return ms;

        }

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

        [ContextMethod("ОтправитьДвоичныеДанные", "SendBinaryData")]
        public void SendString(BinaryDataContext data)
        {
            if (data.Buffer.Length == 0)
                return;

            var stream = _client.GetStream();
            stream.Write(data.Buffer, 0, data.Buffer.Length);
            stream.Flush();

        }

        [ContextProperty("Активно","IsActive")]
        public bool IsActive
        {
            get { return _client.Connected; }
        }

        [ContextProperty("ТаймаутОтправки", "WriteTimeout")]
        public int WriteTimeout
        {
            get { return _client.GetStream().WriteTimeout; }
            set { _client.GetStream().WriteTimeout = value; }
        }

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

        [ContextMethod("Закрыть","Close")]
        public void Close()
        {
            _client.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
