/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using OneScript.DebugProtocol;
using OneScript.Language;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using StackFrame = OneScript.DebugProtocol.StackFrame;
using Variable = OneScript.DebugProtocol.Variable;
using MachineVariable = ScriptEngine.Machine.Variable;

namespace oscript.DebugServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class WcfDebugService : IDebuggerService
    {
        private WcfDebugController Controller { get; }
        
        public WcfDebugService(WcfDebugController controller)
        {
            Controller = controller;
        }

        #region WCF Communication methods

        public void Execute(int threadId)
        {
            RegisterEventListener();
            if (threadId > 0)
            {
                var token = Controller.GetTokenForThread(threadId);
                token.Machine.PrepareDebugContinuation();
                token.ThreadEvent.Set();        
            }
            else
            {
                var tokens = Controller.GetAllTokens();
                foreach (var token in tokens)
                {
                    token.Machine.PrepareDebugContinuation();
                    token.ThreadEvent.Set();
                }
            }
        }

        private void RegisterEventListener()
        {
            var eventChannel = OperationContext.Current.
                   GetCallbackChannel<IDebugEventListener>();

            Controller.SetCallback(eventChannel);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            var confirmedBreakpoints = new List<Breakpoint>();

            var grouped = breaksToSet.GroupBy(g => g.Source);

            foreach (var item in grouped)
            {
                var lines = item
                    .Where(x => x.Line != 0)
                    .Select(x => x.Line)
                    .ToArray();

                foreach (var machine in Controller.GetAllTokens().Select(x=>x.Machine))
                {
                    machine.SetBreakpointsForModule(item.Key, lines);
                }

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

        public StackFrame[] GetStackFrames(int threadId)
        {
            var machine = Controller.GetTokenForThread(threadId).Machine;
            var frames = machine.GetExecutionFrames();
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

        public Variable[] GetVariables(int threadId, int frameId, int[] path)
        {
            var machine = Controller.GetTokenForThread(threadId).Machine;
            var locals = machine.GetFrameLocals(frameId);

            foreach (var step in path)
            {
                var variable = locals[step];
                locals = GetChildVariables(variable);
            }

            return GetDebugVariables(locals);
        }

        public Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path)
        {
            var machine = Controller.GetTokenForThread(threadId).Machine;
            var srcVariable = Evaluate(threadId, frameIndex, expression);

            IValue value;

            try
            {
                value = GetMachine(threadId).Evaluate(expression, true);
            }
            catch (Exception e)
            {
                value = ValueFactory.Create(e.Message);
            }

            var locals = GetChildVariables(MachineVariable.Create(value, "$eval"));

            foreach (var step in path)
            {
                var variable = locals[step];
                locals = GetChildVariables(variable);
            }

            return GetDebugVariables(locals);
        }
        
        private Variable[] GetDebugVariables(IList<IVariable> machineVariables)
        {
            var result = new Variable[machineVariables.Count];

            for (int i = 0; i < machineVariables.Count; i++)
            {
                string presentation;
                string typeName;

                var currentVar = machineVariables[i];

                //На случай проблем, подобных:
                //https://github.com/EvilBeaver/OneScript/issues/918

                try
                {
                    presentation = currentVar.AsString();
                }
                catch (Exception e)
                {
                    presentation = e.Message;
                }

                try
                {
                    typeName = currentVar.SystemType.Name;
                }
                catch (Exception e)
                {
                    typeName = e.Message;
                }

                result[i] = new Variable()
                {
                    Name = currentVar.Name,
                    IsStructured = IsStructured(currentVar),
                    Presentation = presentation,
                    TypeName = typeName
                };
            }

            return result;
        }

        private List<IVariable> GetChildVariables(IVariable src)
        {
            var variables = new List<IVariable>();

            if (HasProperties(src))
            {
                var obj = src.AsObject();
                var propsCount = obj.GetPropCount();
                for (int i = 0; i < propsCount; i++)
                {
                    string propName = obj.GetPropName(i);

                    IVariable value;

                    try
                    {
                        value = MachineVariable.Create(obj.GetPropValue(i), propName);
                    }
                    catch (Exception e)
                    {
                        value = MachineVariable.Create(ValueFactory.Create(e.Message), propName);
                    }

                    variables.Add(value);

                }
            }
            else if(src.AsObject() is IEnumerable<KeyAndValueImpl> collection)
            {
                var propsCount = collection.Count();
                foreach (var kv in collection)
                {
                    IVariable value;

                    try
                    {
                        value = MachineVariable.Create(kv.Value, kv.Key.AsString());
                    }
                    catch (Exception e)
                    {
                        value = MachineVariable.Create(ValueFactory.Create(e.Message), kv.Key.AsString());
                    }

                    variables.Add(value);
                }
            }

            if (HasIndexes(src))
            {
                var obj = src.AsObject();

                if(obj is ICollectionContext cntx)
                {
                    var itemsCount = cntx.Count();
                    for (int i = 0; i < itemsCount; i++)
                    {
                        IValue value;

                        try
                        {
                            value = obj.GetIndexedValue(ValueFactory.Create(i));
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        variables.Add(MachineVariable.Create(value, i.ToString()));
                    }
                }
            }

            return variables;
        }

        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            try
            {
                var value = GetMachine(threadId).Evaluate(expression, true);
                return new Variable()
                {
                    Name = "$evalResult",
                    Presentation = value.AsString(),
                    TypeName = value.SystemType.Name,
                    IsStructured = IsStructured(MachineVariable.Create(value, "$eval"))
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

        public void Next(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepOver();
            t.ThreadEvent.Set();
        }

        public void StepIn(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepIn();
            t.ThreadEvent.Set();
        }

        public void StepOut(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepOut();
            t.ThreadEvent.Set();
        }

        public int[] GetThreads()
        {
            return Controller.GetAllThreadIds();
        }

        private MachineInstance GetMachine(int threadId)
        {
            return Controller.GetTokenForThread(threadId).Machine;
        }

        private bool IsStructured(IVariable variable)
        {
            var result = HasProperties(variable) || HasIndexes(variable);
            if(!result)
            {
                if (variable.DataType == DataType.Object)
                {
                    var obj = variable.AsObject();
                    result = obj is IEnumerable<KeyAndValueImpl>;
                }
            }
            return result;
        }

        private bool HasIndexes(IValue variable)
        {
            if (variable.DataType == DataType.Object)
            {
                var obj = variable.AsObject();
                if (!(obj is IEnumerable<KeyAndValueImpl>)
                    && obj is IRuntimeContextInstance cntx && cntx.IsIndexed)
                {
                    if (obj is ICollectionContext collection)
                    {
                        return collection.Count() > 0;
                    }
                }
            }

            return false;
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
    }
}
