/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
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
            var client = listener.AcceptTcpClient();
            var channel = new BinaryChannel(client);
            listener.Stop();
            IncomingChannel = channel;
        }

        public IDebugController CreateDebugController()
        {
            var ipcServer = new DefaultMessageServer<RpcCall>(IncomingChannel);
            var callback = new TcpEventCallbackChannel(IncomingChannel);
            var threadManager = new ThreadManager();
            var debuggerService = new DefaultDebugService(threadManager, new DefaultVariableVisualizer());
            var controller = new DefaultDebugController(ipcServer, debuggerService, callback, threadManager);

            return controller;
        }

        public ICommunicationChannel IncomingChannel { get; private set; }
    }
}