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

using OneScript.DebugProtocol;

namespace DebugServer
{
    internal class DebugEventListener
    {
        private Thread _networkThread;
        //private readonly Action<EngineDebugEvent> _eventReceivedHandler;
        private readonly TcpClient _client;
        private readonly AutoResetEvent _readWaitEvent = new AutoResetEvent(false);
        private ConcurrentQueue<string> _q = new ConcurrentQueue<string>();

        private bool _listenerCancelled;

        public DebugEventListener(TcpClient client/*, Action<EngineDebugEvent> handler*/)
        {
            //_eventReceivedHandler = handler;
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
                        //var msg = EngineDebugEvent.Deserialize<EngineDebugEvent>(stream);

                        //if (msg.EventType == DebugEventType.ProcessExited)
                        //{
                        //    Stop();
                        //    return;
                        //}

                        //_eventReceivedHandler(msg);
                        _readWaitEvent.Set();
                        
                    }
                    catch
                    {
                        // ошибки возникают только при обрыве соединения
                        Stop();
                    }
                });

                _readWaitEvent.WaitOne();
            }
        }
    }
}
