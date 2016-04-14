using OneScript.Core;
using OneScript.Runtime.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptStackMachine
    {
        MachineMemory _mem;
        CompiledModule _module;
        Stack<CallStackFrame> _callStack = new Stack<CallStackFrame>();
        private Stack<IValue> _operationStack = new Stack<IValue>();
        
        public OneScriptStackMachine()
        {
            Commands = new Action<int>[]
            {
                Nop,
                PushVar,
                PushConst,
                PushLocal,
                LoadVar,
                LoadLocal
            };
        }

        public void AttachTo(OneScriptProcess process)
        {
            _mem = process.Memory;
        }

        public void SetCode(CompiledModule module)
        {
            _module = module;
        }

        public void Run(ModuleMethodDefinition entryPoint)
        {
            Reset();
            Enter(entryPoint);
            Loop();
        }

        public Action<int>[] Commands;

        private void Nop(int arg)
        {
            NextInstruction();
        }

        private void PushVar(int arg)
        {
            var binding = _module.VariableUsageMap[arg];
            var scope = _mem[binding.Context];
            _operationStack.Push(scope.ValueOf(binding.IndexInContext));
            NextInstruction();
        }

        private void PushConst(int arg)
        {
            var constant = _module.Constants.Values[arg];
            _operationStack.Push(constant);
            NextInstruction();
        }

        private void PushLocal(int arg)
        {
            _operationStack.Push(CurrentFrame.Locals[arg].Value);
            NextInstruction();
        }

        private void LoadVar(int arg)
        {
            var binding = _module.VariableUsageMap[arg];
            var scope = _mem[binding.Context];
            scope.ValueRefs[arg].Value = _operationStack.Pop();
            NextInstruction();
        }

        private void LoadLocal(int arg)
        {
            var value = _operationStack.Pop();
            CurrentFrame.Locals[arg].Value = value;
            NextInstruction();
        }

        private void Loop()
        {
            while (_callStack.Count > 0)
            {
                while(CurrentFrame.InstructionPointer != CompiledModule.InvalidEntityIndex 
                    && CurrentFrame.InstructionPointer < _module.Commands.Count)
                {
                    var command = _module.Commands[CurrentFrame.InstructionPointer];
                    var action = Commands[(int)command.Code];
                    action(command.Argument);
                }
                PopFrame();
            }
        }
        
        private void Reset()
        {
            _callStack.Clear();
            _operationStack.Clear();
            CurrentFrame = null;
        }

        private void NextInstruction()
        {
            CurrentFrame.InstructionPointer++;
        }

        public CallStackFrame CurrentFrame
        {
            get;
            private set;
        }

        public void Enter(ModuleMethodDefinition method)
        {
            var frame = new CallStackFrame(_module, method);
            PushFrame(frame);
        }
        
        private void PushFrame(CallStackFrame frame)
        {
            _callStack.Push(frame);
            CurrentFrame = frame;
            _module = CurrentFrame.ModuleInternal;
            _module.Constants.LoadEntities();
        }

        private CallStackFrame PopFrame()
        {
            var frame = _callStack.Pop();
            if (_callStack.Count > 0)
            {
                CurrentFrame = _callStack.Peek();
                _module = CurrentFrame.ModuleInternal;
            }
            else
            {
                CurrentFrame = null;
            }

            return frame;
        }
    }
}
