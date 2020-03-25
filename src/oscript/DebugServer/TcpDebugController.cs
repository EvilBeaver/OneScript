/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Net.Sockets;
using System.Threading;
using OneScript.DebugProtocol;
using ScriptEngine;
using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal class TcpDebugController : DebugControllerBase
    {
        private TcpEventCallbackChannel _eventChannel;
        private TcpChannel _commandsChannel;
        
        public TcpDebugController(int listenerPort)
        {
            Port = listenerPort;
        }

        private int Port { get; }
        
        public override void Init()
        {
            base.Init();
            var listener = TcpListener.Create(Port);
            listener.Start();

            SystemLogger.Write("Initializing debugger");
            try
            {
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    var channel = new TcpChannel(client);
                    var data = channel.Read<string>();
                    if (data == TcpChannelName.Commands && _commandsChannel == null)
                    {
                        _commandsChannel = channel;
                    }
                
                    if (data == TcpChannelName.Events && _eventChannel == null)
                    {
                        _eventChannel = new TcpEventCallbackChannel(channel);
                    }

                    if (_commandsChannel != null && _eventChannel != null)
                    {
                        break;
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
            
            SystemLogger.Write("Debug started");
        }

        protected override void OnMachineStopped(MachineInstance machine, MachineStopReason reason)
        {
            var handle = GetTokenForThread(Thread.CurrentThread.ManagedThreadId);
            handle.ThreadEvent.Reset();
            _eventChannel.ThreadStopped(1, ConvertStopReason(reason));
            handle.ThreadEvent.Wait();
        }

        protected override void Dispose(bool disposing)
        {
            _commandsChannel?.Dispose();
            _eventChannel?.Dispose();
            base.Dispose(disposing);
            _commandsChannel = null;
            _eventChannel = null;
        }
    }
}