using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    class DebugCommandCommunicator
    {
        private const string QUIT_MESSAGE = "quit";

        private IDebugController _controller;
        private TcpListener _socket;
        private TcpClient _connection;

        private ConcurrentQueue<string> _q = new ConcurrentQueue<string>();
        private AutoResetEvent _queueAddedEvent = new AutoResetEvent(false);

        private bool _isStopped;

        public bool GetCommand(out string command)
        {
            if (_isStopped || _connection == null)
            {
                command = null;
                return false;
            }

            if (GetCommandFromQueue(out command))
                return true;

            ThreadPool.QueueUserWorkItem(GetMessageFromNetwork);

            _queueAddedEvent.WaitOne();

            return GetCommandFromQueue(out command);
        }

        private void GetMessageFromNetwork(object state)
        {
            using (var stream = _connection.GetStream())
            {
                var fmt = new BinaryFormatter();
                var msg = (string)fmt.Deserialize(stream);
                if (msg == null)
                {
                    // TODO: when it happens?
                    PostMessage(QUIT_MESSAGE);
                    return;
                }

                PostMessage(msg);
            }
        }

        private void PostMessage(string message)
        {
            _q.Enqueue(message);
            _queueAddedEvent.Set();
        }

        private bool GetCommandFromQueue(out string command)
        {
            if (_q.TryDequeue(out command))
            {
                if (command == QUIT_MESSAGE)
                    return false;

                return true;
            }

            return false;
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
            PostMessage(QUIT_MESSAGE);
            _socket.Stop();
            _connection.Close();
            _connection = null;
            _isStopped = true;
        }

        public void Send(string command)
        {
            using (var stream = _connection.GetStream())
            {
                var fmt = new BinaryFormatter();
                fmt.Serialize(stream, command);
            }
        }
    }
}
