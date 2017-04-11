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
        private IDebugController _controller;
        private TcpListener _tcpListener;
        private TcpClient _eventChannel;

        private readonly object _eventChannelLock = new object();

        public bool GetCommand(out DebugAdapterCommand command)
        {
            while(true)
            {
                try
                {
                    var commandChannel = _tcpListener.AcceptTcpClient();

                    DebugProtocolMessage message;
                    lock (_tcpListener) // не разрешим самим себе закрывать коннект во время чтения
                    {
                        message = DebugProtocolMessage.Deserialize<DebugProtocolMessage>(commandChannel.GetStream());
                    }

                    if (message.Name == "Listener")
                    {
                        EventChannelOperation(commandChannel, (string) message.Data);
                        continue;
                    }

                    command = new DebugAdapterCommand();
                    command.Client = commandChannel;
                    command.Message = message;
                    
                    return true;
                    
                }
                catch (IOException)
                {
                    // операция чтения прервана (кем-то)
                    // клиент отключился, это сообщение мы выбрасываем
                    // и ждем следующего
                }
                catch (SocketException)
                {
                    // сокет ожидащий соединений закрыт. (нами)
                    command = null;
                    return false;
                }
            }
            
        }

        private void EventChannelOperation(TcpClient commandChannel, string operation)
        {
            lock (_eventChannelLock)
            {
                switch (operation)
                {
                    case "start":
                        _eventChannel?.Close();
                        _eventChannel = commandChannel;
                        break;
                    case "stop":
                        _eventChannel?.Close();
                        _eventChannel = null;
                        break;
                }
            }
        }

        public void Start(IDebugController controller, int port)
        {
            _controller = controller;

            var endPoint = new IPEndPoint(IPAddress.Loopback, port);
            _tcpListener = new TcpListener(endPoint);
            _tcpListener.Start(1);
            
        }

        public void Stop()
        {
            lock (_tcpListener)
            {
                _tcpListener.Stop();
                EventChannelOperation(_eventChannel, "stop");
            }
        }

        public void Send(DebugProtocolMessage message)
        {
            lock (_eventChannelLock)
            {
                if (_eventChannel == null)
                    throw new InvalidOperationException("event channel is not open");

                DebugProtocolMessage.Serialize(_eventChannel.GetStream(), message);
            }
        }
    }
}
