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
            var enc = GetEncodingByName(encoding);
            var stream = _client.GetStream();
            
            byte[] readBuffer = new byte[1024];
            StringBuilder completeMessage = new StringBuilder();

            do
            {
                int numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                completeMessage.Append(enc.GetString(readBuffer, 0, numberOfBytesRead));
            } while (stream.DataAvailable);

            return completeMessage.ToString();

        }

        [ContextMethod("ПрочитатьДвоичныеДанные", "ReadBinaryData")]
        public BinaryDataContext ReadBinaryData(int len = 0)
        {
            var stream = _client.GetStream();
            
            bool useLimit = len > 0;
                
            MemoryStream ms = new MemoryStream();
            byte[] readBuffer = new byte[1024];
            do
            {
                int portion = useLimit ? Math.Min(len, readBuffer.Length) : readBuffer.Length;

                int numberOfBytesRead = stream.Read(readBuffer, 0, portion);
                ms.Write(readBuffer, 0, numberOfBytesRead);
                if(useLimit)
                    len -= numberOfBytesRead;

            } while (stream.DataAvailable);

            var data = ms.ToArray();

            return new BinaryDataContext(data);
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
