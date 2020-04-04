/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VSCode.DebugAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showTrace = false;

            foreach (var argument in args)
            {
                switch (argument)
                {
                    case "-trace":
                        showTrace = true;
                        break;
                }
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += (s, e) =>
            {
                SessionLog.WriteLine(e.ExceptionObject.ToString());
            };

            StartSession(showTrace, Console.OpenStandardInput(), Console.OpenStandardOutput());
        }
        
        private static void StartSession(bool showTrace, Stream input, Stream output)
        {
            var session = new OscriptDebugSession();
            session.TRACE = showTrace;
            session.TRACE_RESPONSE = showTrace;
            SessionLog.Open(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +  "/debug.log");
            try
            {
                session.Start(input, output);
            }
            catch (Exception e)
            {
                SessionLog.WriteLine(e.ToString());
            }
            finally
            {
                SessionLog.Close();
            }
        }


#if DEBUG
        private static void RunServer(int port)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            serverSocket.Start();

            new System.Threading.Thread(() => {
                while (true)
                {
                    var clientSocket = serverSocket.AcceptSocket();
                    if (clientSocket != null)
                    {
                        Console.Error.WriteLine(">> accepted connection from client");

                        new System.Threading.Thread(() => {
                            using (var networkStream = new NetworkStream(clientSocket))
                            {
                                try
                                {
                                    StartSession(true, networkStream, networkStream);
                                }
                                catch (Exception e)
                                {
                                    Console.Error.WriteLine("Exception: " + e);
                                }
                            }
                            clientSocket.Close();
                            Console.Error.WriteLine(">> client connection closed");
                        }).Start();
                    }
                }
            }).Start();
        }
#endif
    }
}
