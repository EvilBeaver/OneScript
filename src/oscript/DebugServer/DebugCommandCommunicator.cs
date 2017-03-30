using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using OneScript.DebugProtocol;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    class DebugCommandCommunicator
    {
        private class QuitEvent : DebugProtocolMessage { }
        private readonly QuitEvent QUIT_MESSAGE = new QuitEvent();

        private IDebugController _controller;
        private TcpListener _socket;
        private TcpClient _connection;

        private ConcurrentQueue<DebugProtocolMessage> _q = new ConcurrentQueue<DebugProtocolMessage>();
        private AutoResetEvent _queueAddedEvent = new AutoResetEvent(false);

        private bool _isStopped;

        public bool GetCommand(out DebugProtocolMessage command)
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
            var stream = _connection.GetStream();
            var msg = DebugProtocolMessage.Deserialize<DebugProtocolMessage>(stream);
            PostMessage(msg);
        }

        private void PostMessage(DebugProtocolMessage message)
        {
            _q.Enqueue(message);
            _queueAddedEvent.Set();
        }

        private bool GetCommandFromQueue(out DebugProtocolMessage command)
        {
            if (_q.TryDequeue(out command))
            {
                if (command is QuitEvent)
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
            Send(new EngineDebugEvent(DebugEventType.ProcessExited));
            _socket.Stop();
            _connection.Close();
            _connection = null;
            _isStopped = true;
        }

        public void Send(EngineDebugEvent dbgEvent)
        {
            EngineDebugEvent.Serialize(_connection.GetStream(), dbgEvent);
        }
    }
}
