using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    class OscriptDebugController : IDebugController
    {
        private Thread _listenerThread;
        private readonly ManualResetEventSlim _debugCommandEvent = new ManualResetEventSlim();
        private readonly DebugCommandCommunicator _connection = new DebugCommandCommunicator();

        private readonly int _port;

        public OscriptDebugController(int listenerPort)
        {
            _port = listenerPort;
        }

        public void WaitForDebugEvent(DebugEvent theEvent)
        {
            switch (theEvent)
            {
                case DebugEvent.BeginExecution:
                    _listenerThread = new Thread(ListenerThreadProc);
                    _listenerThread.Start();
                    _debugCommandEvent.Wait(); // процесс 1скрипт не стартует, пока не получено разрешение от дебагера
                    break;
                default:
                    throw new InvalidOperationException($"event {theEvent} cant't be waited");
            }

        }

        public void NotifyProcessExit()
        {
            _connection.Send("exited");
            _connection.Stop();
        }

        private void ListenerThreadProc()
        {
            Output.WriteLine("start listening");
            _connection.Start(this, _port);

            try
            {
                string command;
                while (_connection.GetCommand(out command))
                {
                    Output.WriteLine("DBG Listener:\n" + command);
                    if(command == "go")
                        _debugCommandEvent.Set();
                }
            }
            catch (Exception e)
            {
                Output.WriteLine("DBG Listener:\n" + e.ToString());
                throw;
            }
        }
    }
}
