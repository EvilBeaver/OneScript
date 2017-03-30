using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DebugServer
{
    internal class DebugEventListener
    {
        private Thread _networkThread;
        private readonly Action<string, string> _eventReceivedHandler;
        private readonly TcpClient _client;
        private readonly AutoResetEvent _readWaitEvent = new AutoResetEvent(false);
        private ConcurrentQueue<string> _q = new ConcurrentQueue<string>();

        private bool _listenerCancelled;

        public DebugEventListener(TcpClient client, Action<string, string> handler)
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
            _readWaitEvent.Set();
            SessionLog.WriteLine("event listener stopped");
        }

        private void ListenerThreadBody()
        {
            while (!_listenerCancelled)
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        var stream = _client.GetStream();
                        var reader = new BinaryReader(stream, Encoding.UTF8, true);
                        var msg = reader.ReadString();
                        if (msg == "onStop")
                        {
                            Stop();
                            return;
                        }

                        _eventReceivedHandler("event", msg);
                        _readWaitEvent.Set();
                        
                    }
                    catch (Exception e)
                    {
                        _eventReceivedHandler("error", e.ToString());
                        Stop();
                    }
                });

                _readWaitEvent.WaitOne();
            }
        }
    }
}
