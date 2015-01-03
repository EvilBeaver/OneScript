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
    [ContextClass("TCPСервер", "TCPServer")]
    public class TCPServer : AutoContext<TCPServer>
    {
        private TcpListener _listener;

        public TCPServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        [ContextMethod("Запустить", "Start")]
        public void Start()
        {
            _listener.Start();
        }

        [ContextMethod("Остановить", "Stop")]
        public void Stop()
        {
            _listener.Stop();
        }

        [ContextMethod("ОжидатьСоединения","WaitForConnection")]
        public TCPClient WaitForConnection()
        {
            var client = _listener.AcceptTcpClient();
            return new TCPClient(client);
        }

        [ScriptConstructor]
        public static TCPServer ConstructByPort(IValue port)
        {
            return new TCPServer((int)port.AsNumber());
        }
    }
}
