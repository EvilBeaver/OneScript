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
            executor.DebugController = new OscriptDebugController(_port);

            return executor.Execute();
        }
        
       
    }
}