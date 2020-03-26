/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Threading;

using oscript.DebugServer;

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
            executor.DebugController = new WcfDebugController(_port);

            return executor.Execute();
        }

        public static AppBehavior Create(CmdLineHelper helper)
        {
            var arg = helper.Next();
            int port = 2801;
            if (arg != null && arg.StartsWith("-port="))
            {
                var prefixLen = ("-port=").Length;
                if (arg.Length > prefixLen)
                {
                    var value = arg.Substring(prefixLen);
                    if (!Int32.TryParse(value, out port))
                    {
                        Output.WriteLine("Incorrect port: " + value);
                        return null;
                    }
                }
            }
            else if(arg != null)
            {
                var path = arg;
                return new DebugBehavior(port, path, helper.Tail());
            }

            return null;
        }
    }
}
