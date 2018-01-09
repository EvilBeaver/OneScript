/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

using OneScript.DebugProtocol;

using ScriptEngine;
using ScriptEngine.Machine;

using Variable = OneScript.DebugProtocol.Variable;

namespace oscript.DebugServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    class OscriptDebugController : IDebugController, IDebuggerService
    {
        private readonly ManualResetEventSlim _debugCommandEvent = new ManualResetEventSlim();

        private IDebugEventListener _eventChannel;
        private readonly int _port;
        private MachineInstance _machine;

        private ServiceHost _serviceHost;

        private readonly DebuggerFSM _debugFSM;

        public OscriptDebugController(int listenerPort)
        {
            _port = listenerPort;

            _debugFSM = new DebuggerFSM();

            var initialState = new BeforeExecutionState(this);
            var runningState = new RunningState(this);
            var stoppedState = new StoppedState(this);

            _debugFSM.AddState(initialState, DebuggerCommands.Execute, runningState);
            _debugFSM.AddState(initialState, DebuggerCommands.Help, initialState);
            _debugFSM.AddState(initialState, DebuggerCommands.SetBreakpoint, initialState);
            _debugFSM.AddState(runningState, DebuggerCommands.OutgoingEvent, stoppedState);
        }

        public void WaitForDebugEvent(DebugEventType theEvent)
        {
            switch (theEvent)
            {
                case DebugEventType.BeginExecution:

                    //var host = new ServiceHost(this);
                    //var binding = Binder.GetBinding();
                    //host.AddServiceEndpoint(typeof(IDebuggerService), binding, Binder.GetDebuggerUri(_port));
                    //_serviceHost = host;
                    //host.Open();


                    _debugFSM.Start();
                    _debugCommandEvent.Wait(); // процесс 1скрипт не стартует, пока не получено разрешение от дебагера

                    break;
                default:
                    throw new InvalidOperationException($"event {theEvent} cant't be waited");
            }

        }

        public void NotifyProcessExit(int exitCode)
        {
            if (!CallbackChannelIsReady())
                return; // нет подписчика

            _eventChannel.ProcessExited(exitCode);
            _serviceHost?.Close();
        }

        public void OnMachineReady(MachineInstance instance)
        {
            _machine = instance;
            _machine.MachineStopped += MachineStopHanlder;
        }

        private void MachineStopHanlder(object sender, MachineStoppedEventArgs e)
        {
            if (!CallbackChannelIsReady())
                return; // нет подписчика
            
            _debugCommandEvent.Reset();
            _eventChannel.ThreadStopped(1, ConvertStopReason(e.Reason));
            _debugCommandEvent.Wait();
        }

        private ThreadStopReason ConvertStopReason(MachineStopReason reason)
        {
            switch(reason)
            {
                case MachineStopReason.Breakpoint:
                    return ThreadStopReason.Breakpoint;
                case MachineStopReason.Step:
                    return ThreadStopReason.Step;
                case MachineStopReason.Exception:
                    return ThreadStopReason.Exception;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool CallbackChannelIsReady()
        {
            if (_eventChannel != null)
            {
                var ico = (ICommunicationObject) _eventChannel;
                return ico.State == CommunicationState.Opened;
            }
            return false;
        }

        #region WCF Communication methods

        public void Execute()
        {
            RegisterEventListener();
            _machine.PrepareDebugContinuation();
            _debugCommandEvent.Set();
        }

        private void RegisterEventListener()
        {
            _eventChannel = OperationContext.Current.
                   GetCallbackChannel<IDebugEventListener>();
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            var confirmedBreakpoints = new List<Breakpoint>();
            
            var grouped = breaksToSet.GroupBy(g => g.Source);

            foreach (var item in grouped)
            {
                var lines = item
                    .Where(x=>x.Line != 0)
                    .Select(x => x.Line)
                    .ToArray();

                _machine.SetBreakpointsForModule(item.Key, lines);

                foreach (var line in lines)
                {
                    confirmedBreakpoints.Add(new Breakpoint()
                    {
                        Line = line,
                        Source = item.Key
                    });
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

        public OneScript.DebugProtocol.Variable[] GetVariables(int frameId, int[] path)
        {
            var locals =_machine.GetFrameLocals(frameId);
            foreach (var step in path)
            {
                var variable = locals[step];
                if (HasProperties(variable))
                {
                    var obj = variable.AsObject();
                    locals = new List<IVariable>();
                    var propsCount = obj.GetPropCount();
                    for (int i = 0; i < propsCount; i++)
                    {
                        string propName = obj.GetPropName(i);

                        try
                        {
                            locals.Add(ScriptEngine.Machine.Variable.Create(obj.GetPropValue(i), propName));
                        }
                        catch(Exception e)
                        {
                            locals.Add(ScriptEngine.Machine.Variable.Create(ValueFactory.Create(e.Message), propName));
                        }
                        
                    }
                }
            }

            var result = new OneScript.DebugProtocol.Variable[locals.Count];
            for (int i = 0; i < locals.Count; i++)
            {
                result[i] = new OneScript.DebugProtocol.Variable()
                {
                    Name = locals[i].Name,
                    IsStructured = HasProperties(locals[i]),
                    Presentation = locals[i].AsString(),
                    TypeName = locals[i].SystemType.Name
                };
            }

            return result;
        }

        public Variable Evaluate(int contextFrame, string expression)
        {
            try
            {
                var value = _machine.Evaluate(expression, true);
                return new Variable()
                {
                    Name = "$evalResult",
                    Presentation = value.AsString(),
                    TypeName = value.SystemType.Name,
                    IsStructured = HasProperties(value)
                };
            }
            catch (ScriptException e)
            {
                return new Variable()
                {
                    Name = "$evalFault",
                    Presentation = e.ErrorDescription,
                    TypeName = "Ошибка",
                    IsStructured = false
                };
            }
        }

        public void Next()
        {
            _machine.StepOver();
            _debugCommandEvent.Set();
        }

        public void StepIn()
        {
            _machine.StepIn();
            _debugCommandEvent.Set();
        }

        public void StepOut()
        {
            _machine.StepOut();
            _debugCommandEvent.Set();
        }

        private static bool HasProperties(IValue variable)
        {
            if (variable.DataType == DataType.Object)
            {
                var obj = variable.AsObject();
                return obj.GetPropCount() > 0;
            }

            return false;
        }

        #endregion

        public void InputCommand()
        {
            var thread = new Thread(InteractiveInput);
            thread.Start();
        }

        private void InteractiveInput()
        {
            bool selected = false;
            while (!selected)
            {
                Output.Write($"{_debugFSM.CurrentState.Prompt}>");
                var line = Console.ReadLine();
                var parser = new CmdLineHelper(SplitArguments(line));
                var commandName = parser.Next();
                DebuggerCommands command;

                if (_debugFSM.SelectCommand(commandName, out command))
                {
                    selected = true;
                    _debugFSM.DispatchCommand(command, parser.Tail());
                }
                else
                {
                    Output.WriteLine($"Неизвестная команда {commandName}");
                }    
            }
            
        }

        private string[] SplitArguments(string line)
        {
            return CommandsParser.SplitCommandLine(line);
        }
    }
}
