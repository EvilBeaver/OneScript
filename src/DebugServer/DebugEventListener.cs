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
        private readonly Action<DebugProtocolMessage> _eventReceivedHandler;
        private readonly TcpClient _client;
        
        private bool _listenerCancelled;

        public DebugEventListener(TcpClient client, Action<DebugProtocolMessage> handler)
        {
            _eventReceivedHandler = handler;
            _client = client;
        }

        public void Start()
        {
            _networkThread = new Thread(ListenerThreadBody);
            _networkThread.IsBackground = true;
            _networkThread.Start();
            SessionLog.WriteLine("event listener started");
        }

        public void Stop()
        {
            _listenerCancelled = true;
            SessionLog.WriteLine("event listener stopped");
        }

        private void ListenerThreadBody()
        {
            while (!_listenerCancelled)
            {
                try
                {
                    var stream = _client.GetStream();
                    var message = DebugProtocolMessage.Deserialize<DebugProtocolMessage>(stream);
                }
                catch(IOException e)
                {
                    // socket closed
                    _listenerCancelled = true;
                    _eventReceivedHandler(new DebugProtocolMessage()
                    {
                        Name = "Internal error: listener stopped",
                        Data = e
                    });
                }
            }
        }
    }
}
