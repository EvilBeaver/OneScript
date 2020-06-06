/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Threading;
using OneScript.DebugServices;
using ScriptEngine;
using ScriptEngine.Machine;

namespace oscript
{
    internal class DebugBehavior : AppBehavior
    {
        private readonly string[] _args;
        private readonly string _path;
        private readonly int _port;
        
        public DebugBehavior(int port, string path, string[] args)
        {
            _args = args;
            _path = path;
            _port = port;
        }

        public override int Execute()
        {
            var executor = new ExecuteScriptBehavior(_path, _args);
            SystemLogger.SetWriter(executor);
            var tcpDebugServer = new BinaryTcpDebugServer(_port);
                    executor.DebugController = tcpDebugServer.CreateDebugController();
            
            return executor.Execute();
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
