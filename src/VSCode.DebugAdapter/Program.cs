/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace VSCode.DebugAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--debug"))
            {
                var listener = TcpListener.Create(4711);
                listener.Start();

                using var client = listener.AcceptTcpClient();
                using var stream = client.GetStream();

                StartSession(stream, stream);
            }
            else
                StartSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        }
        
        private static void StartSession(Stream input, Stream output)
        {
            var session = new OscriptDebugSession();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
                .CreateLogger();
            
            try
            {
                Log.Logger.Information("Starting debug adapter");
                session.Start(input, output);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Exception on session start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
            
            Log.Logger.Information("Session completed");
        }
    }
}
