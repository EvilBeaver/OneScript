/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.DebugProtocol;
using OneScript.Language;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using StackFrame = OneScript.DebugProtocol.StackFrame;
using Variable = OneScript.DebugProtocol.Variable;
using MachineVariable = ScriptEngine.Machine.Variable;

namespace oscript.DebugServer
{
    internal class DebugServiceImpl : IDebuggerService
    {
        private DebugControllerBase Controller { get; }

        public DebugServiceImpl(DebugControllerBase controller)
        {
            Controller = controller;
        }
        
        public void Execute(int threadId)
        {
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

        public virtual Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
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

        public virtual StackFrame[] GetStackFrames(int threadId)
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

        private MachineInstance GetMachine(int threadId)
        {
            return Controller.GetTokenForThread(threadId).Machine;
        }

        
        public virtual Variable[] GetVariables(int threadId, int frameIndex, int[] path)
        {
            var machine = Controller.GetTokenForThread(threadId).Machine;
            var locals = machine.GetFrameLocals(frameIndex);

            foreach (var step in path)
            {
                var variable = locals[step];
                locals = GetChildVariables(variable);
            }

            return GetDebugVariables(locals);
        }

        public virtual Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path)
        {
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

        public virtual Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            try
            {
                var value = GetMachine(threadId).Evaluate(expression, true);
                
                var variable = CreateDebuggerVariable("$evalResult",
                    value.AsString(), value.SystemType.Name);

                variable.IsStructured = IsStructured(MachineVariable.Create(value, "$eval"));

                return variable;
            }
            catch (ScriptException e)
            {
                return CreateDebuggerVariable("$evalFault", e.ErrorDescription,
                    "Ошибка");
            }
        }

        public virtual void Next(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepOver();
            t.ThreadEvent.Set();
        }

        public virtual void StepIn(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepIn();
            t.ThreadEvent.Set();
        }

        public virtual void StepOut(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepOut();
            t.ThreadEvent.Set();
        }

        public virtual int[] GetThreads()
        {
            return Controller.GetAllThreadIds();
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

                result[i] = CreateDebuggerVariable(currentVar.Name, presentation, typeName);
                result[i].IsStructured = IsStructured(currentVar);
            }

            return result;
        }
        
        public Variable CreateDebuggerVariable(string name, string presentation, string typeName)
        {
            if (presentation.Length > DebuggerSettings.MAX_PRESENTATION_LENGTH)
                presentation = presentation.Substring(0, DebuggerSettings.MAX_PRESENTATION_LENGTH) + "...";

            return new Variable()
            {
                Name = name,
                Presentation = presentation,
                TypeName = typeName
            };
        }

        private List<IVariable> GetChildVariables(IVariable src)
        {
            var variables = new List<IVariable>();

            if (HasProperties(src))
            {
                FillProperties(src, variables);
            }
            else if(src.AsObject() is IEnumerable<KeyAndValueImpl> collection)
            {
                FillKeyValueProperties(collection, variables);
            }

            if (HasIndexes(src))
            {
                FillIndexedProperties(src, variables);
            }

            return variables;
        }

        private void FillKeyValueProperties(IEnumerable<KeyAndValueImpl> collection, List<IVariable> variables)
        {
            var propsCount = collection.Count();

            int i = 0;

            foreach (var kv in collection)
            {
                IVariable value;

                value = MachineVariable.Create(kv, i.ToString());

                variables.Add(value);

                i++;
            }
        }

        private void FillIndexedProperties(IVariable src, List<IVariable> variables)
        {
            var obj = src.AsObject();

            if (obj is ICollectionContext cntx)
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

        private void FillProperties(IVariable src, List<IVariable> variables)
        {
            var obj = src.AsObject();
            var propsCount = obj.GetPropCount();
            for (int i = 0; i < propsCount; i++)
            {
                var propNum = i;
                var propName = obj.GetPropName(propNum);
                
                IVariable value;

                try
                {
                    value = MachineVariable.Create(obj.GetPropValue(propNum), propName);
                }
                catch (Exception e)
                {
                    value = MachineVariable.Create(ValueFactory.Create(e.Message), propName);
                }

                variables.Add(value);
            }
        }
        
        private bool IsStructured(IVariable variable)
        {
            var result = HasProperties(variable) || HasIndexes(variable);
            if(!result)
            {
                if (VariableHasType(variable, DataType.Object))
                {
                    var obj = variable.AsObject();
                    result = obj is IEnumerable<KeyAndValueImpl>;
                }
            }
            return result;
        }

        private bool HasIndexes(IValue variable)
        {
            if (VariableHasType(variable, DataType.Object))
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
            if (!VariableHasType(variable, DataType.Object))
                return false;
            var obj = variable.AsObject();
            return obj.GetPropCount() > 0;
        }

        private static bool VariableHasType(IValue variable, DataType type)
        {
            return variable.GetRawValue() != null && variable.DataType == type;
        }
    }
}