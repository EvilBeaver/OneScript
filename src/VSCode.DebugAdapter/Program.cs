/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Configuration;
using System.IO;
using System.Linq;
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

                using (var client = listener.AcceptTcpClient())
                {
                    using (var stream = client.GetStream())
                    {
                        StartSession(stream, stream);
                    }
                }
                
            }
            else
                StartSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        }
        
        private static void StartSession(Stream input, Stream output)
        {
            var enabled = ConfigurationManager.AppSettings["onescript:log-enable"].ToLower() == "true";
            var file = ConfigurationManager.AppSettings["serilog:write-to:File.path"];
            if (enabled && !string.IsNullOrEmpty(file))
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.AppSettings()
                    .Enrich.FromLogContext()
                    .WriteTo.File(file, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}){NewLine}{Exception}")
                    .CreateLogger();
            }
            
            var session = new OscriptDebugSession();
            
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
