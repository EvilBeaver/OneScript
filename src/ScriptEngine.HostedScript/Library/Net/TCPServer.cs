using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Net
{
    public class TCPServer : AutoContext<TCPServer>
    {
        private TcpListener _listener;

        public TCPServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Listen(string handler)
        {
            
        }

        [ScriptConstructor]
        public static TCPServer ConstructByPort(IValue port)
        {
            return new TCPServer((int)port.AsNumber());
        }
    }
}
