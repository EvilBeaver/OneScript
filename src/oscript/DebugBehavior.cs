using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using ScriptEngine.HostedScript.Library.Net;
using ScriptEngine.Machine;

namespace oscript
{
    internal class DebugBehavior : AppBehavior, IDebugController
    {
        private readonly string[] _args;
        private readonly string _path;
        private readonly int _port;

        private Thread _listenerThread;
        private ManualResetEventSlim _debugCommandEvent;
        private DebugCommandDispatcher _dispatcher = new DebugCommandDispatcher();

        public DebugBehavior(int port, string path, string[] args)
        {
            _args = args;
            _path = path;
            _port = port;
            _debugCommandEvent = new ManualResetEventSlim();
        }

        public override int Execute()
        {
            var executor = new ExecuteScriptBehavior(_path, _args);
            executor.DebugController = this;

            return executor.Execute();
        }

        public void WaitForExecutionSignal()
        {
            _listenerThread = new Thread(ListenerThreadProc);
            _listenerThread.Start();
            _debugCommandEvent.Wait(); // процесс 1скрипт не стартует, пока не получено разрешение от дебагера

        }

        public void NotifyProcessExit()
        {
            _dispatcher.Post("stop");
        }

        private void ListenerThreadProc()
        {
            _dispatcher.Start(this, _port);

            try
            {
                string command;
                while (_dispatcher.GetCommand(out command))
                {
                    switch (command)
                    {
                        case "start":
                            _debugCommandEvent.Set();
                            break;
                        case "stop":
                            _dispatcher.Stop();
                            break;
                    }
                }
            }
            catch(Exception e)
            {
                Output.WriteLine("DBG Listener:\n" + e.ToString());
                throw;
            }
        }
    }
}