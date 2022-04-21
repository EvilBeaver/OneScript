/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.DebugServices;
using ScriptEngine;

namespace oscript
{
    internal class DebugBehavior : ExecuteScriptBehavior
    {
        private readonly int _port;
        
        public DebugBehavior(int port, string path, string[] args) : base(path, args)
        {
            _port = port;
        }

        public override int Execute()
        {
            var tcpDebugServer = new BinaryTcpDebugServer(_port);
                    
            DebugController = tcpDebugServer.CreateDebugController();
            
            return base.Execute();
        }

        public static AppBehavior Create(CmdLineHelper helper)
        {
            int port = 2801;
            string path = null;
            
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
                else if (parsedArg.Name == "-protocol")
                {
                    continue;
                }
                else
                {
                    path = arg;
                    break;
                }
            }

            return path == null ? null : new DebugBehavior(port, path, helper.Tail());
        }
    }
}
