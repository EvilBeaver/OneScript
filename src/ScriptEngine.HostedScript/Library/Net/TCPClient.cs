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
            using (var stream = _client.GetStream())
            {
                byte[] readBuffer = new byte[1024];
                StringBuilder completeMessage = new StringBuilder();

                do
                {
                    int numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    completeMessage.Append(enc.GetString(readBuffer, 0, numberOfBytesRead));
                } while (stream.DataAvailable);

                return completeMessage.ToString();

            }
        }

        [ContextMethod("ПрочитатьДвоичныеДанные", "ReadBinaryData")]
        public BinaryDataContext ReadBinaryData()
        {
            using (var stream = _client.GetStream())
            {
                MemoryStream ms = new MemoryStream();
                byte[] readBuffer = new byte[1024];
                do
                {
                    int numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    ms.Write(readBuffer, 0, numberOfBytesRead);
                    
                } while (stream.DataAvailable);

                var data = ms.ToArray();

                return new BinaryDataContext(data);

            }
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
