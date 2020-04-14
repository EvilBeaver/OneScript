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
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using StackFrame = OneScript.DebugProtocol.StackFrame;
using Variable = OneScript.DebugProtocol.Variable;
using MachineVariable = ScriptEngine.Machine.Variable;

namespace OneScript.DebugServices
{
    public class DefaultDebugService : IDebuggerService
    {
        private readonly IVariableVisualizer _visualizer;
        private ThreadManager _threadManager { get; }

        public DefaultDebugService(ThreadManager threads, IVariableVisualizer visualizer)
        {
            _visualizer = visualizer;
            _threadManager = threads;
        }
        
        public void Execute(int threadId)
        {
            if (threadId > 0)
            {
                var token = _threadManager.GetTokenForThread(threadId);
                token.Machine.PrepareDebugContinuation();
                token.Set();        
            }
            else
            {
                var tokens = _threadManager.GetAllTokens();
                foreach (var token in tokens)
                {
                    token.Machine.PrepareDebugContinuation();
                    token.Set();
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

                foreach (var machine in _threadManager.GetAllTokens().Select(x=>x.Machine))
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
            var machine = _threadManager.GetTokenForThread(threadId).Machine;
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
            return _threadManager.GetTokenForThread(threadId).Machine;
        }

        public virtual Variable[] GetVariables(int threadId, int frameIndex, int[] path)
        {
            var machine = _threadManager.GetTokenForThread(threadId).Machine;
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
            var t = _threadManager.GetTokenForThread(threadId);
            t.Machine.StepOver();
            t.Set();
        }

        public virtual void StepIn(int threadId)
        {
            var t = _threadManager.GetTokenForThread(threadId);
            t.Machine.StepIn();
            t.Set();
        }

        public virtual void StepOut(int threadId)
        {
            var t = _threadManager.GetTokenForThread(threadId);
            t.Machine.StepOut();
            t.Set();
        }

        public virtual int[] GetThreads()
        {
            return _threadManager.GetAllThreadIds();
        }
        
        private Variable[] GetDebugVariables(IList<IVariable> machineVariables)
        {
            return machineVariables.Select(x => _visualizer.GetVariable(x))
                .ToArray();
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

            if (VariableHasType(src, DataType.Object))
            {
                var context = src.AsObject();
                if (context is IEnumerable<IValue> collection)
                {
                    FillIndexedProperties(collection, variables);
                }
            }

            return variables;
        }

        private void FillIndexedProperties(IEnumerable<IValue> collection, List<IVariable> variables)
        {
            int index = 0;
            foreach (var collectionItem in collection)
            {
                variables.Add(MachineVariable.Create(collectionItem, index.ToString()));
                ++index;
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
            return HasProperties(variable) || HasIndexes(variable);
        }

        private bool HasIndexes(IValue variable)
        {
            if (VariableHasType(variable, DataType.Object))
            {
                var obj = variable.AsObject();
                if (obj is ICollectionContext collection)
                {
                    return collection.Count() > 0;
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