/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.TcpServer;
using ScriptEngine;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class BinaryTcpDebugServer
    {
        public void WaitForConnections(int port)
        {
            var listener = TcpListener.Create(port);
            listener.Start();
            SystemLogger.Write("Initializing debugger");
            try
            {
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    var channel = new BinaryChannel(client);
                    var data = channel.Read<string>();
                    if (data == DebugChannelName.Commands && IncomingChannel == null)
                    {
                        IncomingChannel = channel;
                    }

                    if (data == DebugChannelName.Events && OutcomingChannel == null)
                    {
                        OutcomingChannel = channel;
                    }

                    if (IncomingChannel != null && OutcomingChannel != null)
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

        public IDebugController CreateDebugController()
        {
            var ipcServer = new DefaultMessageServer<TcpProtocolDto>(IncomingChannel);
            var callback = new TcpEventCallbackChannel(OutcomingChannel);
            var threadManager = new ThreadManager();
            var debuggerService = new DefaultDebugService(threadManager, new DefaultVariableVisualizer());
            var controller = new DefaultDebugController(ipcServer, debuggerService, callback, threadManager);

            return controller;
        }

        public BinaryChannel IncomingChannel { get; private set; }
        
        public BinaryChannel OutcomingChannel { get; private set; }
    }
}