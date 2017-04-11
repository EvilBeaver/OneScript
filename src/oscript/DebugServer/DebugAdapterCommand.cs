using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using OneScript.DebugProtocol;

namespace oscript.DebugServer
{
    internal class DebugAdapterCommand : IDisposable
    {
        public TcpClient Client { get; set; }
        public DebugProtocolMessage Message { get; set; }

        public void Dispose()
        {
            Client?.Close();
        }
    }
}
