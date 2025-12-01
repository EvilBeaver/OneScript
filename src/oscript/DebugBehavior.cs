/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.DebugProtocol;
using OneScript.DebugServices;

namespace oscript
{
    internal class DebugBehavior : ExecuteScriptBehavior
    {
        public DebugBehavior(string path, string[] args) : base(path, args)
        {
        }

        public int Port { get; set; } = 2801;
        
        public bool AttachMode { get; set; } = true;

        public override int Execute()
        {
            var tcpDebugServer = new TcpDebugServer(Port);
            
            DebugController = new DefaultDebugger(tcpDebugServer)
            {
                AttachMode = AttachMode
            };
            
            return base.Execute();
        }

        public static AppBehavior Create(CmdLineHelper helper)
        {
            int port = 2801;
            string path = null;
            bool noWait = false;
            
            while (true)
            {
                var arg = helper.Next();
                if (arg == null)
                {
                    break;
                }

                var parsedArg = helper.Parse(arg);
                if (parsedArg.Name == "-port")
                {
                    var portString = parsedArg.Value;
                    if (string.IsNullOrEmpty(portString)) 
                        return null;
                
                    if (!Int32.TryParse(portString, out port))
                    {
                        Output.WriteLine("Incorrect port: " + portString);
                        return null;
                    }
                }
                else if (parsedArg.Name == "-noWait")
                {
                    noWait = true;
                }
                else if (parsedArg.Name == "-protocol")
                {
                    // Обратная совместимость, не используется в реальности
                    continue;
                }
                else
                {
                    path = arg;
                    break;
                }
            }

            return path == null ? null : new DebugBehavior(path, helper.Tail())
            {
                Port = port, AttachMode = noWait
            };
        }
    }
}
