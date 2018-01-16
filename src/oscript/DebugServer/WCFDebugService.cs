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

using ScriptEngine;
using ScriptEngine.Machine;

using StackFrame = OneScript.DebugProtocol.StackFrame;
using Variable = OneScript.DebugProtocol.Variable;

namespace oscript.DebugServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class WcfDebugService : IDebuggerService
    {
        private WcfDebugController Controller { get; }

        public MachineInstance Machine { get; set; }

        public WcfDebugService()
        {
            Controller = DebugBehavior.GetDebugController() as WcfDebugController;
            Debug.Assert(Controller != null, "Debug behavior is not created for WCF-debug channel");
        }

        #region WCF Communication methods

        public void Execute()
        {
            RegisterEventListener();
            Machine.PrepareDebugContinuation();
            Controller.ThreadEvent.Set();
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

                Machine.SetBreakpointsForModule(item.Key, lines);

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
            var frames = Machine.GetExecutionFrames();
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
            var locals = Machine.GetFrameLocals(frameId);
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
                        catch (Exception e)
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
                var value = Machine.Evaluate(expression, true);
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
            Machine.StepOver();
            Controller.ThreadEvent.Set();
        }

        public void StepIn()
        {
            Machine.StepIn();
            Controller.ThreadEvent.Set();
        }

        public void StepOut()
        {
            Machine.StepOut();
            Controller.ThreadEvent.Set();
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
