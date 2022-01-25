/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Net.Sockets;
using OneScript.DebugProtocol.TcpServer;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class BinaryTcpDebugServer
    {
        private readonly int _port;

        public BinaryTcpDebugServer(int port)
        {
            _port = port;
        }

        public IDebugController CreateDebugController()
        {
            var listener = TcpListener.Create(_port);
            var channel = new DelayedConnectionChannel(listener);
            var ipcServer = new DefaultMessageServer<RpcCall>(channel);
            var callback = new TcpEventCallbackChannel(channel);
            var threadManager = new ThreadManager();
            var breakpoints = new DefaultBreakpointManager();
            var debuggerService = new DefaultDebugService(breakpoints, threadManager, new DefaultVariableVisualizer());
            var controller = new DefaultDebugController(ipcServer, debuggerService, callback, threadManager, breakpoints);

            return controller;
        }
    }
}