/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using OneScript.DebugProtocol.TcpServer;

namespace OneScript.DebugProtocol.Test.Tools
{
    public class TestDebuggerClient
    {
        private readonly TcpClient _client = new TcpClient();
        
        public void Connect(int port)
        {
            _client.Connect(new IPEndPoint(IPAddress.Loopback, port));
            var handshake = FormatReconcileUtils.GetReconcileMagic();
            _client.GetStream().Write(handshake);
            _client.GetStream().Flush();
            
            var buffer = new byte[FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length + sizeof(int)];
            StreamUtils.ReadStream(_client.GetStream(), buffer, buffer.Length);
        }

        public void Send(RpcCall message)
        {
            var content = JsonConvert.SerializeObject(message);
            var contentBytes = Encoding.UTF8.GetBytes(content);

            using var bufferedStream = new MemoryStream(contentBytes.Length + sizeof(int));
            using var writer = new BinaryWriter(bufferedStream, Encoding.UTF8);
            
            writer.Write(contentBytes.Length);
            writer.Write(contentBytes, 0, contentBytes.Length);

            bufferedStream.Position = 0;
            bufferedStream.CopyTo(_client.GetStream());
            _client.GetStream().Flush();
        }
    }
}