using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

using OneScript.DebugProtocol;
using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class OscriptDebugController : IDebugController, IDebuggerService
    {
        private readonly ManualResetEventSlim _debugCommandEvent = new ManualResetEventSlim();

        private IDebugEventListener _eventChannel;
        private readonly int _port;
        private MachineInstance _machine;

        private ServiceHost _serviceHost;

        public OscriptDebugController(int listenerPort)
        {
            _port = listenerPort;
        }

        public void WaitForDebugEvent(DebugEventType theEvent)
        {
            switch (theEvent)
            {
                case DebugEventType.BeginExecution:

                    var host = new ServiceHost(this);
                    var binding = Binder.GetBinding();
                    host.AddServiceEndpoint(typeof(IDebuggerService), binding, Binder.GetDebuggerUri(_port));
                    _serviceHost = host;
                    host.Open();
                    
                    _debugCommandEvent.Wait(); // процесс 1скрипт не стартует, пока не получено разрешение от дебагера
                    break;
                default:
                    throw new InvalidOperationException($"event {theEvent} cant't be waited");
            }

        }

        public void NotifyProcessExit()
        {
            _serviceHost?.Close();
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

            if (_eventChannel == null)
                return; // нет подписчика
            
            _debugCommandEvent.Reset();
            _eventChannel.ThreadStopped(1, ThreadStopReason.Breakpoint);
            _debugCommandEvent.Wait();
        }
        
        #region WCF Communication methods

        public void Execute()
        {
            Output.WriteLine("execute received");
            _debugCommandEvent.Set();
        }

        public void RegisterEventListener()
        {
            _eventChannel = OperationContext.Current.
                   GetCallbackChannel<IDebugEventListener>();
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            var confirmedBreakpoints = new List<Breakpoint>();

            foreach (var bpt in breaksToSet)
            {
                int id;
                if (_machine.SetBreakpoint(bpt.Source, bpt.Line, out id))
                {
                    bpt.Id = id;
                    confirmedBreakpoints.Add(bpt);
                }
            }

            return confirmedBreakpoints.ToArray();
        }

        public StackFrame[] GetStackFrames()
        {
            var frames = _machine.GetExecutionFrames();
            var result = new StackFrame[frames.Count];
            int index = 0;
            foreach (var frameInfo in frames)
            {
                var frame = new StackFrame();
                frame.LineNumber = frameInfo.LineNumber;
                frame.Index = index++;
                frame.MethodName = frameInfo.MethodName;
                frame.Source = frameInfo.Source;
                result[frame.Index] = frame;

            }

            return result;
        }

        public OneScript.DebugProtocol.Variable[] GetVariables(int frameId)
        {
            var locals =_machine.GetFrameLocals(frameId);
            var result = new OneScript.DebugProtocol.Variable[locals.Count];
            for (int i = 0; i < locals.Count; i++)
            {
                result[i] = new OneScript.DebugProtocol.Variable()
                {
                    Name = locals[i].Name,
                    IsStructured = locals[i].DataType == DataType.Object,
                    Presentation = locals[i].AsString(),
                    TypeName = locals[i].SystemType.Name
                };
            }

            return result;
        }

        #endregion
    }
}
