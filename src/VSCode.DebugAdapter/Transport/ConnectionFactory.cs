/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Net.Sockets;
using System.Threading;
using Serilog;
using Serilog.Core;

namespace VSCode.DebugAdapter.Transport
{
    public static class ConnectionFactory
    {
        private static ILogger Log { get; set; } = Logger.None;
        
        public static TcpClient Connect(int port)
        {
            Log = Serilog.Log.ForContext(typeof(ConnectionFactory));
            
            var debuggerUri = GetDebuggerUri(port); 
            
            var client = new TcpClient();
            TryConnect(client, debuggerUri);
            
            return client;
        }
        
        private static Uri GetDebuggerUri(int port)
        {
            var builder = new UriBuilder();
            builder.Scheme = "net.tcp";
            builder.Port = port;
            builder.Host = "localhost";

            return builder.Uri;
        }

        private static void TryConnect(TcpClient client, Uri debuggerUri)
        {
            const int limit = 3;
            // TODO: параметризовать ожидания и попытки
            for (int i = 0; i < limit; ++i)
            {
                try
                {
                    client.Connect(debuggerUri.Host, debuggerUri.Port);
                    break;
                }
                catch (SocketException)
                {
                    if (i == limit - 1)
                        throw;
                    
                    Log.Warning("Error. Retry connect {Attempt}", i);
                    Thread.Sleep(1500);
                }
            }
            
            Log.Debug("Connected to {Host}:{Port}", debuggerUri.Host, debuggerUri.Port);
        }
    }
}
