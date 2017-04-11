using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OneScript.DebugProtocol;

namespace DebugServer
{
    internal class DebugEventListener
    {
        private Thread _networkThread;
        private TcpClient _client;
        private readonly int _tcpPort;

        public DebugEventListener(int port)
        {
            _tcpPort = port;
        }

        public event Action<DebugEventListener, DebugProtocolMessage> DebugEventReceived;

        public void Start()
        {
            _client = new TcpClient();
            var initMsg = new DebugProtocolMessage()
            {
                Name = "Listener",
                Data = "start"
            };
            SessionLog.WriteLine("start exe->adapter listener");
            _client.Connect("localhost", _tcpPort);
            DebugProtocolMessage.Serialize(_client.GetStream(), initMsg);

            _networkThread = new Thread(ListenerThreadBody);
            _networkThread.IsBackground = true;
            _networkThread.Start();
            SessionLog.WriteLine("event listener started");
        }

        public void Stop()
        {
            if (_client != null)
            {
                _client = new TcpClient();
                var initMsg = new DebugProtocolMessage()
                {
                    Name = "Listener",
                    Data = "stop"
                };
                DebugProtocolMessage.Serialize(_client.GetStream(), initMsg);

                _client.Close();
            }
            SessionLog.WriteLine("event listener stopped");
        }

        private void ListenerThreadBody()
        {
            while (true)
            {
                try
                {
                    var stream = _client.GetStream();
                    var message = DebugProtocolMessage.Deserialize<DebugProtocolMessage>(stream);
                    OnDebugEventReceived(message);
                }
                catch(IOException e)
                {
                    // socket closed
                    OnDebugEventReceived(new DebugProtocolMessage()
                    {
                        Name = "Internal error: listener stopped",
                        Data = e
                    });
                    break;
                }
            }
        }

        private void OnDebugEventReceived(DebugProtocolMessage msg)
        {
            DebugEventReceived?.Invoke(this, msg);
        }
    }
}
