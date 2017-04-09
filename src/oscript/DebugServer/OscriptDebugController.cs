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
            _machine.MachineStopped += MachineStopHanlder;
        }

        private void MachineStopHanlder(object sender, MachineStoppedEventArgs e)
        {
            if (e.Reason != MachineStopReason.Breakpoint)
                throw new NotImplementedException("Not implemented yet");

            var message = new DebugProtocolMessage()
            {
                Name = "Breakpoint",
                Data = 1 // thread id
            };

            _debugCommandEvent.Reset();
            _connection.Send(message);
            _debugCommandEvent.Wait();
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
            Output.WriteLine($"got command: {command.Name}");
            switch (command.Name)
            {
                case "BeginExecution":
                    _debugCommandEvent.Set();
                    break;
                case "SetBreakpoints":
                    SetMachineBreakpoints(command.Data as Breakpoint[]);
                    break;
            }
        }

        private void SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            List<Breakpoint> confirmedBreakpoints = new List<Breakpoint>();

            foreach (var bpt in breaksToSet)
            {
                int id;
                if (_machine.SetBreakpoint(bpt.Source, bpt.Line, out id))
                {
                    bpt.Id = id;
                    confirmedBreakpoints.Add(bpt);
                }
            }

            var message = new DebugProtocolMessage()
            {
                Name = "ConfirmedBreakpoints",
                Data = confirmedBreakpoints.ToArray()
            };

            _connection.Send(message);
        }
    }
}
