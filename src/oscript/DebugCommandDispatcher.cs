using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

using ScriptEngine.Machine;

namespace oscript
{
    class DebugCommandDispatcher
    {
        private IDebugController _controller;
        private TcpListener _socket;
        private TcpClient _connection;

        private bool _isStopped = false;

        public bool GetCommand(out string command)
        {
            if (_isStopped || _connection == null)
            {
                command = null;
                return false;
            }

            using (var stream = _connection.GetStream())
            {
                var fmt = new BinaryFormatter();
                command = (string) fmt.Deserialize(stream);
                return true;
            }

        }

        public void Start(IDebugController controller, int port)
        {
            _controller = controller;

            var endPoint = new IPEndPoint(IPAddress.Loopback, port);
            _socket = new TcpListener(endPoint);
            _socket.Start(1);

            _connection = _socket.AcceptTcpClient();

        }

        public void Stop()
        {
            _socket.Stop();
            _connection.Close();
            _connection = null;
            _isStopped = true;
        }

        public void Post(string stop)
        {
            throw new NotImplementedException();
        }
    }
}
