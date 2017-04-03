using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using OneScript.DebugProtocol;

using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    class OscriptDebugController : IDebugController
    {
        private Thread _listenerThread;
        private readonly ManualResetEventSlim _debugCommandEvent = new ManualResetEventSlim();
        private readonly DebugCommandCommunicator _connection = new DebugCommandCommunicator();

        private readonly int _port;
        private MachineInstance _machine;

        public OscriptDebugController(int listenerPort)
        {
            _port = listenerPort;
        }

        public void WaitForDebugEvent(DebugEventType theEvent)
        {
            switch (theEvent)
            {
                case DebugEventType.BeginExecution:
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
            Console.WriteLine("Sending stop to debug listener");
            _connection.Stop();
        }

        public void OnMachineReady(MachineInstance instance)
        {
            _machine = instance;
        }

        private void ListenerThreadProc()
        {
            Output.WriteLine("start listening");
            _connection.Start(this, _port);

            try
            {
                DebugProtocolMessage command;
                while (_connection.GetCommand(out command))
                {
                    DispatchMessage(command);
                }
            }
            catch (Exception e)
            {
                Output.WriteLine("DBG Listener:\n" + e.ToString());
                throw;
            }
        }

        private void DispatchMessage(DebugProtocolMessage command)
        {
            if (command is EngineDebugEvent)
            {
                var edb = (EngineDebugEvent) command;
                if(edb.EventType == DebugEventType.BeginExecution)
                    _debugCommandEvent.Set();
            }
            else if (command.Type == MessageType.Command)
            {
                if (command is SetSourceBreakpointsCommand)
                {
                    
                }
            }
        }
    }
}
