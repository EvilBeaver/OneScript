/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine.Contexts;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ScriptEngine.Compiler;
using ScriptEngine.Environment;

namespace ScriptEngine.Machine
{
    public class MachineInstance
    {
        private List<Scope> _scopes;
        private Stack<IValue> _operationStack;
        private Stack<ExecutionFrame> _callStack;
        private ExecutionFrame _currentFrame;
        private Action<int>[] _commands;
        private Stack<ExceptionJumpInfo> _exceptionsStack;
        private Stack<MachineState> _states;
        private LoadedModule _module;
        private ICodeStatCollector _codeStatCollector;
        private MachineStopManager _stopManager;

        // для отладчика.
        // актуален в момент останова машины
        private IList<ExecutionFrameInfo> _fullCallstackCache;

        internal MachineInstance() 
        {
            InitCommands();
            Reset();
        }

        public event EventHandler<MachineStoppedEventArgs> MachineStopped;

        private struct ExceptionJumpInfo
        {
            public int handlerAddress;
            public ExecutionFrame handlerFrame;
        }

        private struct MachineState
        {
            public Scope topScope;
            public LoadedModule module;
            public bool hasScope;
            public IValue[] operationStack;
        }

        public void AttachContext(IAttachableContext context, bool detachable)
        {
            IVariable[] vars;
            MethodInfo[] methods;
            context.OnAttach(this, out vars, out methods);
            var scope = new Scope()
            {
                Variables = vars,
                Methods = methods,
                Instance = context,
                Detachable = detachable
            };

            _scopes.Add(scope);

        }

        internal void StateConsistentOperation(Action action)
        {
            PushState();
            try
            {
                action();
            }
            finally
            {
                PopState();
            }
        }

        internal void ExecuteModuleBody()
        {
            if (_module.EntryMethodIndex >= 0)
            {
                var entryRef = _module.MethodRefs[_module.EntryMethodIndex];
                PrepareMethodExecutionDirect(entryRef.CodeIndex);
                ExecuteCode();
            }
        }

        internal IValue ExecuteMethod(int methodIndex, IValue[] arguments)
        {
            PrepareMethodExecutionDirect(methodIndex);
            var method = _module.Methods[methodIndex];
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is IVariable)
                    _currentFrame.Locals[i] = Variable.CreateReference((IVariable)arguments[i], method.Variables[i]);
                else if(arguments[i] == null)
                    _currentFrame.Locals[i] = Variable.Create(GetDefaultArgValue(methodIndex, i), method.Variables[i]);
                else
                    _currentFrame.Locals[i] = Variable.Create(arguments[i], method.Variables[i]);
            }
            ExecuteCode();

            if (_module.Methods[methodIndex].Signature.IsFunction)
            {
                return _operationStack.Pop();
            }

            return null;
        }

        #region Debug protocol methods

        public void SetDebugMode(IDebugController debugContr)
        {
            _stopManager = new MachineStopManager();
        }

        public bool SetBreakpoint(string source, int line, out int id)
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

            id = _stopManager.SetBreakpoint(source, line);
            
            return true;
        }

        public void StepOver()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

            //_stopManager.StepOver(_currentFrame);
        }

        public void StepIn()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

           // _stopManager.AddStopAtMethodEntry();
        }

        public void PrepareDebugContinuation()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");
            // ??
            //_stopManager.ClearSteppingStops();
        }

        public IValue Evaluate(string expression, bool separate = false)
        {
            var code = CompileExpressionModule(expression);

            MachineInstance runner;
            if (separate)
            {
                runner = new MachineInstance();
                runner._scopes = new List<Scope>(_scopes);
            }
            else
                runner = this;

            var frame = new ExecutionFrame();
            frame.MethodName = code.ModuleInfo.ModuleName;
            frame.Locals = new IVariable[0];
            frame.InstructionPointer = 0;
            var curModule = _module;

            var mlocals = new Scope();
            mlocals.Instance = new UserScriptContextInstance(code);
            mlocals.Detachable = true;
            mlocals.Methods = TopScope.Methods;
            mlocals.Variables = _currentFrame.Locals;
            runner._scopes.Add(mlocals);

            try
            {
                if (!separate)
                    PushFrame(frame);

                //runner.SetFrame(frame);
                runner.SetModule(code);
                runner.MainCommandLoop();
            }
            finally
            {
                if (!separate)
                {
                    DetachTopScope(out mlocals);
                    PopFrame();
                    SetModule(curModule);
                }
            }

            var result = runner._operationStack.Pop();

            return result;

        }


        #endregion

        internal ScriptInformationContext CurrentScript
        {
            get
            {
                if (_module.ModuleInfo != null)
                    return new ScriptInformationContext(_module.ModuleInfo);
                else
                    return null;
            }
        }

        private IValue GetDefaultArgValue(int methodIndex, int paramIndex)
        {
            var meth = _module.Methods[methodIndex].Signature;
            var param = meth.Params[paramIndex];
            if (!param.HasDefaultValue)
                throw new ApplicationException("Invalid script arguments");

            return _module.Constants[param.DefaultValueIndex];
        }

        internal void SetModule(LoadedModule module)
        {
            _module = module;
        }

        internal void Cleanup()
        {
            Reset();
            GC.Collect();
        }

        private void PushState()
        {
            var stateToSave = new MachineState();
            stateToSave.hasScope = DetachTopScope(out stateToSave.topScope);
            stateToSave.module = _module;
            StackToArray(ref stateToSave.operationStack, _operationStack);

            _states.Push(stateToSave);

            _operationStack.Clear();
        }

        private void StackToArray<T>(ref T[] destination, Stack<T> source)
        {
            if (source != null)
            {
                destination = new T[source.Count];
                source.CopyTo(destination, 0);
            }
        }

        private void RestoreStack<T>(ref Stack<T> destination, T[] source)
        {
            if (source != null)
            {
                destination = new Stack<T>();
                for (int i = source.Length-1; i >=0 ; i--)
                {
                    destination.Push(source[i]);
                }
            }
            else
            {
                destination = null;
            }
        }

        private void PopState()
        {
            var savedState = _states.Pop();
            if (savedState.hasScope)
            {
                if (_scopes[_scopes.Count - 1].Detachable)
                {
                    _scopes[_scopes.Count - 1] = savedState.topScope;
                }
                else
                {
                    _scopes.Add(savedState.topScope);
                }
            }
            else if (_scopes[_scopes.Count - 1].Detachable)
            {
                Scope s;
                DetachTopScope(out s);
            }

            _module = savedState.module;

            RestoreStack(ref _operationStack, savedState.operationStack);

        }

        private void PushFrame(ExecutionFrame frame)
        {
            CodeStat_StopFrameStatistics();
            _callStack.Push(frame);
            _currentFrame = frame;
        }

        private void PopFrame()
        {
            _callStack.Pop();
            _currentFrame = _callStack.Peek();
            CodeStat_ResumeFrameStatistics();
        }

        private bool DetachTopScope(out Scope topScope)
        {
            if (_scopes.Count > 0)
            {
                topScope = _scopes[_scopes.Count - 1];
                if (topScope.Detachable)
                {
                    _scopes.RemoveAt(_scopes.Count - 1);
                    return true;
                }
                else
                {
                    topScope = default(Scope);
                    return false;
                }
            }
            else
            {
                throw new InvalidOperationException("Nothing is attached");
            }
        }

        private Scope TopScope
        {
            get
            {
                if (_scopes.Count > 0)
                {
                    return _scopes[_scopes.Count - 1];
                }
                else
                {
                    throw new InvalidOperationException("Nothing is attached");
                }
            }
        }

        private void Reset()
        {
            _scopes = new List<Scope>();
            _operationStack = new Stack<IValue>();
            _callStack = new Stack<ExecutionFrame>();
            _exceptionsStack = new Stack<ExceptionJumpInfo>();
            _states = new Stack<MachineState>();
            _module = null;
            _currentFrame = null;
        }
        
        private void PrepareMethodExecutionDirect(int methodIndex)
        {
            var methDescr = _module.Methods[methodIndex];
            var frame = new ExecutionFrame();
            frame.MethodName = methDescr.Signature.Name;
            frame.Locals = new IVariable[methDescr.Variables.Count];
            for (int i = 0; i < frame.Locals.Length; i++)
            {
                frame.Locals[i] = Variable.Create(ValueFactory.Create(), methDescr.Variables[i]);
            }

            frame.InstructionPointer = methDescr.EntryPoint;
            PushFrame(frame);
        }

        private void PrepareCodeStatisticsData()
        {
            foreach (var method in _module.Methods)
            {
                var instructionPointer = method.EntryPoint;
                while (instructionPointer < _module.Code.Length)
                {
                    if (_module.Code[instructionPointer].Code == OperationCode.LineNum)
                    {
                        var entry = new CodeStatEntry(
                            CurrentScript?.Source,
                            method.Signature.Name,
                            _module.Code[instructionPointer].Argument
                        );
                        _codeStatCollector.MarkEntryReached(entry, count: 0);
                    }

                    if (_module.Code[instructionPointer].Code == OperationCode.Return)
                    {
                        break;
                    }

                    instructionPointer++;
                }
            }
        }

        private void ExecuteCode()
        {
            if (_codeStatCollector != null)
            {
                if (!_codeStatCollector.IsPrepared(CurrentScript?.Source))
                {
                    PrepareCodeStatisticsData();
                    _codeStatCollector.MarkPrepared(CurrentScript?.Source);
                }
            }

            while (true)
            {
                try
                {
                    MainCommandLoop();
                    break;
                }
                catch (RuntimeException exc)
                {
                    if(exc.LineNumber == 0)
                        SetScriptExceptionSource(exc);

                    if (_exceptionsStack.Count == 0)
                    {
                        throw;
                    }

                    var handler = _exceptionsStack.Pop();

                    // Раскрутка стека вызовов
                    while (_currentFrame != handler.handlerFrame)
                    {
                        PopFrame();
                    }

                    //SetFrame(handler.handlerFrame);
                    _currentFrame.InstructionPointer = handler.handlerAddress;
                    _currentFrame.LastException = exc;
                    

                }
            }
        }

        public void SetCodeStatisticsCollector(ICodeStatCollector collector)
        {
            _codeStatCollector = collector;
        }

        private CodeStatEntry CurrentCodeEntry()
        {
            return new CodeStatEntry(CurrentScript?.Source, _currentFrame.MethodName, _currentFrame.LineNumber);
        }

        private void CodeStat_LineReached()
        {
            if (_codeStatCollector == null)
                return;

            _codeStatCollector.MarkEntryReached(CurrentCodeEntry());
        }

        private void CodeStat_StopFrameStatistics()
        {
            _codeStatCollector?.StopWatch(CurrentCodeEntry());
        }

        private void CodeStat_ResumeFrameStatistics()
        {
            _codeStatCollector?.ResumeWatch(CurrentCodeEntry());
        }

        private void MainCommandLoop()
        {
            try
            {
                while (_currentFrame.InstructionPointer >= 0
                    && _currentFrame.InstructionPointer < _module.Code.Length)
                {
                    var command = _module.Code[_currentFrame.InstructionPointer];
                    _commands[(int)command.Code](command.Argument);
                }
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch(ScriptInterruptionException)
            {
                throw;
            }
            catch (Exception exc)
            {
                var excWrapper = new ExternalSystemException(exc);
                SetScriptExceptionSource(excWrapper);
                throw excWrapper;
            }
        }

        private void SetScriptExceptionSource(RuntimeException exc)
        {
            exc.LineNumber = _currentFrame.LineNumber;
            if (_module.ModuleInfo != null)
            {
                exc.ModuleName = _module.ModuleInfo.ModuleName;
                exc.Code = _module.ModuleInfo.CodeIndexer.GetCodeLine(exc.LineNumber);
            }
            else
            {
                exc.ModuleName = "<имя модуля недоступно>";
                exc.Code = "<исходный код недоступен>";
            }
        }

        #region Commands

        private void InitCommands()
        {
            _commands = new Action<int>[]
            {
                (i)=>{NextInstruction();},
                PushVar,
                PushConst,
                PushLoc,
                PushRef,
                LoadVar,
                LoadLoc,
                AssignRef,
                Add,
                Sub,
                Mul,
                Div,
                Mod,
                Neg,
                Equals,
                Less,
                Greater,
                LessOrEqual,
                GreaterOrEqual,
                NotEqual,
                Not,
                And,
                Or,
                CallFunc,
                CallProc,
                ArgNum,
                PushDefaultArg,
                ResolveProp,
                ResolveMethodProc,
                ResolveMethodFunc,
                Jmp,
                JmpFalse,
                PushIndexed,
                Return,
                JmpCounter,
                Inc,
                NewInstance,
                PushIterator,
                IteratorNext,
                StopIterator,
                BeginTry,
                EndTry,
                RaiseException,
                LineNum,
                MakeRawValue,
                MakeBool,
                PushTmp,
                PopTmp,

                //built-ins
                Eval,
                Bool,
                Number,
                Str,
                Date,
                Type,
                ValType,
                StrLen,
                TrimL,
                TrimR,
                TrimLR,
                Left,
                Right,
                Mid,
                StrPos,
                UCase,
                LCase,
                TCase,
                Chr,
                ChrCode,
                EmptyStr,
                StrReplace,
                StrGetLine,
                StrLineCount,
                StrEntryCount,
                Year,
                Month,
                Day,
                Hour,
                Minute,
                Second,
                BegOfYear,
                BegOfMonth,
                BegOfDay,
                BegOfHour,
                BegOfMinute,
                BegOfQuarter,
                EndOfYear,
                EndOfMonth,
                EndOfDay,
                EndOfHour,
                EndOfMinute,
                EndOfQuarter,
                WeekOfYear,
                DayOfYear,
                this.DayOfWeek,
                AddMonth,
                CurrentDate,
                Integer,
                Round,
                Log,
                Log10,
                Sin,
                Cos,
                Tan,
                ASin,
                ACos,
                ATan,
                Exp,
                Pow,
                Sqrt,
                Min,
                Max,
                Format,
                ExceptionInfo,
                ExceptionDescr,
                ModuleInfo
            };
        }

        #region Simple operations
        private void PushVar(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ContextIndex];
            _operationStack.Push(scope.Variables[vm.CodeIndex]);
            NextInstruction();
        }

        private void PushConst(int arg)
        {
            _operationStack.Push(_module.Constants[arg]);
            NextInstruction();
        }

        private void PushLoc(int arg)
        {
            _operationStack.Push(_currentFrame.Locals[arg]);
            NextInstruction();
        }

        private void PushRef(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ContextIndex];
            var reference = Variable.CreateContextPropertyReference(scope.Instance, vm.CodeIndex, "$stackvar");
            _operationStack.Push(reference);
            NextInstruction();
        }

        private void LoadVar(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ContextIndex];
            scope.Variables[vm.CodeIndex].Value = BreakVariableLink(_operationStack.Pop());
            NextInstruction();
        }

        private void LoadLoc(int arg)
        {
            _currentFrame.Locals[arg].Value = BreakVariableLink(_operationStack.Pop());
            NextInstruction();
        }

        private void AssignRef(int arg)
        {
            var value = BreakVariableLink(_operationStack.Pop());

            IVariable reference;
            try
            {
                reference = (IVariable)_operationStack.Pop();
            }
            catch (InvalidCastException)
            {
                throw new WrongStackConditionException();
            }
            reference.Value = value;
            NextInstruction();
        }

        private void Add(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Add(op1, op2));
            NextInstruction();

        }

        private void Sub(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Sub(op1, op2));
            NextInstruction();
        }

        private void Mul(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Mul(op1, op2));
            NextInstruction();
        }

        private void Div(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Div(op1, op2));
            NextInstruction();
        }

        private void Mod(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Mod(op1, op2));
            NextInstruction();
        }

        private void Neg(int arg)
        {
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Neg(op1));
            NextInstruction();
        }

        private void Equals(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.Equals(op2)));
            NextInstruction();
        }

        private void Less(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) < 0));
            NextInstruction();
        }

        private void Greater(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) > 0));
            NextInstruction();
        }

        private void LessOrEqual(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) <= 0));
            NextInstruction();
        }

        private void GreaterOrEqual(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) >= 0));
            NextInstruction();
        }

        private void NotEqual(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(!op1.Equals(op2)));
            NextInstruction();
        }

        private void Not(int arg)
        {
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(!op1.AsBoolean()));
            NextInstruction();
        }

        private void And(int arg)
        {
            var op = _operationStack.Peek().AsBoolean();
            if (op == false)
            {
                Jmp(arg);
            }
            else
            {
                _operationStack.Pop();
                NextInstruction();
            }
            
        }

        private void Or(int arg)
        {
            var op = _operationStack.Peek().AsBoolean();
            if (op == true)
            {
                Jmp(arg);
            }
            else
            {
                _operationStack.Pop();
                NextInstruction();
            }
        }

        private void CallFunc(int arg)
        {
            bool needsDiscarding = MethodCallImpl(arg, true);
            _currentFrame.DiscardReturnValue = needsDiscarding;
        }

        private void CallProc(int arg)
        {
            bool needsDiscarding = MethodCallImpl(arg, false);
            _currentFrame.DiscardReturnValue = needsDiscarding;
        }

        private bool MethodCallImpl(int arg, bool asFunc)
        {
            var methodRef = _module.MethodRefs[arg];
            var scope = _scopes[methodRef.ContextIndex];
            var methInfo = scope.Methods[methodRef.CodeIndex];

            int argCount = (int)_operationStack.Pop().AsNumber();
            IValue[] argValues = new IValue[argCount];

            // fact args
            for (int i = argCount - 1; i >= 0; i--)
            {
                var argValue = _operationStack.Pop();
                if (argValue.DataType == DataType.NotAValidValue)
                {
                    if (i < methInfo.Params.Length)
                    {
                        var constId = methInfo.Params[i].DefaultValueIndex;
                        if (constId == ParameterDefinition.UNDEFINED_VALUE_INDEX)
                            argValue = null;
                        else
                            argValue = _module.Constants[constId];
                    }
                    else
                    {
                        argValue = null;
                    }
                }

                argValues[i] = argValue;

            }

            bool needsDiscarding;

            if (scope.Instance == this.TopScope.Instance)
            {
                var sdo = scope.Instance as ScriptDrivenObject;
                System.Diagnostics.Debug.Assert(sdo != null);

                if (sdo.MethodDefinedInScript(methodRef.CodeIndex))
                {
                    var methDescr = _module.Methods[sdo.GetMethodDescriptorIndex(methodRef.CodeIndex)];
                    var frame = new ExecutionFrame();
                    frame.MethodName = methInfo.Name;
                    frame.Locals = new IVariable[methDescr.Variables.Count];
                    for (int i = 0; i < frame.Locals.Length; i++)
                    {
                        if (i < argValues.Length)
                        {
                            var paramDef = methInfo.Params[i];
                            if (argValues[i] is IVariable)
                            {
                                if (paramDef.IsByValue)
                                {
                                    var value = ((IVariable)argValues[i]).Value;
                                    frame.Locals[i] = Variable.Create(value, methDescr.Variables[i]);
                                }
                                else
                                {
                                    frame.Locals[i] = Variable.CreateReference((IVariable)argValues[i], methDescr.Variables[i]);
                                }
                            }
                            else
                            {
                                frame.Locals[i] = Variable.Create(argValues[i], methDescr.Variables[i]);
                            }

                        }
                        else if (i < methInfo.Params.Length && methInfo.Params[i].HasDefaultValue)
                        {
                            if (methInfo.Params[i].DefaultValueIndex == ParameterDefinition.UNDEFINED_VALUE_INDEX)
                            {
                                frame.Locals[i] = Variable.Create(ValueFactory.Create(), methDescr.Variables[i]);
                            }
                            else
                            {
                                frame.Locals[i] = Variable.Create(_module.Constants[methInfo.Params[i].DefaultValueIndex], methDescr.Variables[i]);
                            }
                        }
                        else
                            frame.Locals[i] = Variable.Create(ValueFactory.Create(), methDescr.Variables[i]);

                    }

                    frame.InstructionPointer = methDescr.EntryPoint;
                    PushFrame(frame);
                    if (_stopManager != null)
                    {
                        //_stopManager.OnFrameEntered(frame);
                    }

                    needsDiscarding = methInfo.IsFunction && !asFunc;
                }
                else
                {
                    needsDiscarding = _currentFrame.DiscardReturnValue;
                    CallContext(scope.Instance, methodRef.CodeIndex, ref methInfo, argValues, asFunc);
                }

            }
            else
            {
                // при вызове библиотечного метода (из другого scope)
                // статус вызова текущего frames не должен изменяться.
                //
                needsDiscarding = _currentFrame.DiscardReturnValue;
                CallContext(scope.Instance, methodRef.CodeIndex, ref methInfo, argValues, asFunc);
            }

            return needsDiscarding;
        }

        private void CallContext(IRuntimeContextInstance instance, int index, ref MethodInfo methInfo, IValue[] argValues, bool asFunc)
        {
            IValue[] realArgs;
            if (!instance.DynamicMethodSignatures)
            {
                realArgs = new IValue[methInfo.ArgCount];
                for (int i = 0; i < realArgs.Length; i++)
                {
                    if (i < argValues.Length)
                    {
                        realArgs[i] = argValues[i];
                    }
                    else
                    {
                        realArgs[i] = null;
                    }
                }
            }
            else
            {
                realArgs = argValues;
            }

            if (asFunc)
            {
                IValue retVal;
                instance.CallAsFunction(index, realArgs, out retVal);
                _operationStack.Push(retVal);
            }
            else
            {
                instance.CallAsProcedure(index, realArgs);
            }
            NextInstruction();
        }

        private void ArgNum(int arg)
        {
            _operationStack.Push(ValueFactory.Create(arg));
            NextInstruction();
        }

        private void PushDefaultArg(int arg)
        {
            _operationStack.Push(ValueFactory.CreateInvalidValueMarker());
            NextInstruction();
        }

        private void ResolveProp(int arg)
        {
            var objIValue = _operationStack.Pop();
            if (objIValue.DataType != DataType.Object)
            {
                throw RuntimeException.ValueIsNotObjectException();
            }

            var context = objIValue.AsObject();
            var propName = _module.Constants[arg].AsString();
            var propNum = context.FindProperty(propName);

            var propReference = Variable.CreateContextPropertyReference(context, propNum, "stackvar");
            _operationStack.Push(propReference);
            NextInstruction();

        }

        private void ResolveMethodProc(int arg)
        {
            IRuntimeContextInstance context;
            int methodId;
            IValue[] argValues;
            PrepareContextCallArguments(arg, out context, out methodId, out argValues);

            context.CallAsProcedure(methodId, argValues);
            NextInstruction();

        }

        private void ResolveMethodFunc(int arg)
        {
            IRuntimeContextInstance context;
            int methodId;
            IValue[] argValues;
            PrepareContextCallArguments(arg, out context, out methodId, out argValues);

            if (!context.DynamicMethodSignatures && !context.GetMethodInfo(methodId).IsFunction)
            {
                throw RuntimeException.UseProcAsAFunction();
            }

            IValue retVal;
            context.CallAsFunction(methodId, argValues, out retVal);
            _operationStack.Push(retVal);
            NextInstruction();
        }

        private void PrepareContextCallArguments(int arg, out IRuntimeContextInstance context, out int methodId, out IValue[] argValues)
        {
            var argCount = (int)_operationStack.Pop().AsNumber();
            IValue[] factArgs = new IValue[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                factArgs[i] = _operationStack.Pop();
            }

            var objIValue = _operationStack.Pop();
            if (objIValue.DataType != DataType.Object)
            {
                throw RuntimeException.ValueIsNotObjectException();
            }

            context = objIValue.AsObject();
            var methodName = _module.Constants[arg].AsString();
            methodId = context.FindMethod(methodName);
            var methodInfo = context.GetMethodInfo(methodId);

            if(context.DynamicMethodSignatures)
                argValues = new IValue[argCount];
            else
                argValues = new IValue[methodInfo.Params.Length];

            bool[] signatureCheck = new bool[argCount];

            // fact args
            for (int i = 0; i < factArgs.Length; i++)
            {
                var argValue = factArgs[i];
                if (argValue.DataType == DataType.NotAValidValue)
                {
                    argValue = null;
                    signatureCheck[i] = false;
                }
                else
                {
                    signatureCheck[i] = true;
                    if (context.DynamicMethodSignatures)
                    {
                        argValues[i] = BreakVariableLink(argValue);
                    }
                    else if (i < methodInfo.Params.Length)
                    {
                        if (methodInfo.Params[i].IsByValue)
                            argValues[i] = BreakVariableLink(argValue);
                        else
                            argValues[i] = argValue;
                    }
                }

            }
            factArgs = null;
            if (!context.DynamicMethodSignatures)
            {
                CheckFactArguments(methodInfo, signatureCheck);

                //manage default vals
                for (int i = argCount; i < argValues.Length; i++)
                {
                    if (methodInfo.Params[i].HasDefaultValue)
                    {
                        argValues[i] = null;
                    }
                }
            }
        }

        private void CheckFactArguments(MethodInfo methInfo, bool[] argsPassed)
        {
            if (argsPassed.Length > methInfo.Params.Length)
            {
                throw RuntimeException.TooManyArgumentsPassed();
            }

            for (int i = 0; i < methInfo.Params.Length; i++)
            {
                var paramDef = methInfo.Params[i];
                if (i < argsPassed.Length)
                {
                    if (argsPassed[i] == false && !paramDef.HasDefaultValue)
                    {
                        throw RuntimeException.ArgHasNoDefaultValue(i + 1);
                    }
                }
                else if (!paramDef.HasDefaultValue)
                {
                    throw RuntimeException.TooLittleArgumentsPassed();
                }
            }
        }

        private void Jmp(int arg)
        {
            _currentFrame.InstructionPointer = arg;
        }

        private void JmpFalse(int arg)
        {
            var op1 = _operationStack.Pop();

            if (!op1.AsBoolean())
            {
                _currentFrame.InstructionPointer = arg;
            }
            else
            {
                NextInstruction();
            }
        }

        private void PushIndexed(int arg)
        {
            var index = BreakVariableLink(_operationStack.Pop());
            var context = _operationStack.Pop().AsObject();
            if (context == null || !context.IsIndexed)
            {
                throw RuntimeException.IndexedAccessIsNotSupportedException();
            }

            _operationStack.Push(Variable.CreateIndexedPropertyReference(context, index, "$stackvar"));
            NextInstruction();

        }

        private void Return(int arg)
        {
            if (_currentFrame.DiscardReturnValue)
                _operationStack.Pop();

            while(_exceptionsStack.Count > 0 && _exceptionsStack.Peek().handlerFrame == _currentFrame)
            {
                _exceptionsStack.Pop();
            }
            
            PopFrame();
            NextInstruction();
            
        }

        private void JmpCounter(int arg)
        {
            var counter = _operationStack.Pop();
            var limit = _currentFrame.LocalFrameStack.Peek();

            if (counter.CompareTo(limit) <= 0)
            {
                NextInstruction();
            }
            else
            {
                Jmp(arg);
            }
        }

        private void Inc(int arg)
        {
            var operand = _operationStack.Pop().AsNumber();
            operand = operand + 1;
            _operationStack.Push(ValueFactory.Create(operand));
            NextInstruction();
        }

        private void NewInstance(int argCount)
        {
            IValue[] argValues = new IValue[argCount];
            // fact args
            for (int i = argCount - 1; i >= 0; i--)
            {
                var argValue = _operationStack.Pop();
                argValues[i] = BreakVariableLink(argValue);
            }

            var typeName = _operationStack.Pop().AsString();
            var clrType = TypeManager.GetFactoryFor(typeName);

            var ctors = clrType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute))
                .Select(x => new 
                    {   CtorInfo = x,
                        Parametrized = ((ScriptConstructorAttribute)x.GetCustomAttributes(typeof(ScriptConstructorAttribute), false)[0]).ParametrizeWithClassName 
                    });

            foreach (var ctor in ctors)
            {
                var parameters = ctor.CtorInfo.GetParameters();
                List<object> argsToPass = new List<object>();
                if (ctor.Parametrized)
                {
                    if (parameters.Length < 1)
                    {
                        continue;
                    }
                    if (parameters[0].ParameterType != typeof(string))
                    {
                        throw new InvalidOperationException("Type parametrized constructor must have first argument of type String");
                    }
                    argsToPass.Add(typeName);
                    parameters = parameters.Skip(1).ToArray();
                }

                bool success = (parameters.Length == 0 && argCount == 0)
                    ||(parameters.Length > 0 && parameters[0].ParameterType.IsArray);

                if (parameters.Length > 0 && parameters.Length < argCount 
                    && !parameters[parameters.Length-1].ParameterType.IsArray)
                {
                    success = false;
                    continue;
                }
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsArray)
                    {
                        // captures all remained args
                        IValue[] varArgs = new IValue[argCount - i];
                        for (int j = i, k = 0; k < varArgs.Length; j++, k++)
                        {
                            varArgs[k] = argValues[j];
                        }
                        argsToPass.Add(varArgs);
                        success = true;
                        break;
                    }
                    else
                    {
                        if (i < argValues.Length)
                        {

                            if (argValues[i].DataType == DataType.NotAValidValue)
                            {
                                if (parameters[i].IsOptional)
                                    argsToPass.Add(null);
                                else
                                {
                                    throw RuntimeException.ArgHasNoDefaultValue(i + 1);
                                }
                            }
                            else
                                argsToPass.Add(argValues[i]);

                            success = true;
                        }
                        else
                        {
                            if (parameters[i].IsOptional)
                            {
                                argsToPass.Add(null);
                                success = true;
                            }
                            else
                            {
                                success = false;
                                break; // no match
                            }
                        }
                    }
                }

                if (success)
                {
                    object instance = null;
                    try
                    {
                        instance = ctor.CtorInfo.Invoke(null, argsToPass.ToArray());
                    }
                    catch (System.Reflection.TargetInvocationException e)
                    {
                        if (e.InnerException != null)
                            throw e.InnerException;
                        else
                            throw;
                    }

                    _operationStack.Push((IValue)instance);
                    NextInstruction();
                    return;
                }

            }

            throw new RuntimeException("Конструктор не найден ("+typeName+")");


        }

        private void PushIterator(int arg)
        {
            var collection = _operationStack.Pop();
            if (collection.DataType == DataType.Object)
            {
                var context = collection.AsObject() as ICollectionContext;
                if (context == null)
                {
                    throw RuntimeException.IteratorIsNotDefined();
                }

                var iterator = context.GetManagedIterator();
                _currentFrame.LocalFrameStack.Push(iterator);
                NextInstruction();

            }
            else
            {
                throw RuntimeException.ValueIsNotObjectException();
            }
        }

        private void IteratorNext(int arg)
        {
            var iterator = _currentFrame.LocalFrameStack.Peek() as CollectionEnumerator;
            if (iterator == null)
            {
                throw new WrongStackConditionException();
            }

            var hasNext = iterator.MoveNext();
            if (hasNext)
            {
                _operationStack.Push(iterator.Current);
            }
            _operationStack.Push(ValueFactory.Create(hasNext));
            NextInstruction();
        }

        private void StopIterator(int arg)
        {
            var iterator = _currentFrame.LocalFrameStack.Pop() as CollectionEnumerator;
            if (iterator == null)
            {
                throw new WrongStackConditionException();
            }

            iterator.Dispose();
            NextInstruction();
        }

        private void BeginTry(int exceptBlockAddress)
        {
            var info = new ExceptionJumpInfo();
            info.handlerAddress = exceptBlockAddress;
            info.handlerFrame = _currentFrame;

            _exceptionsStack.Push(info);
            NextInstruction();
        }

        private void EndTry(int arg)
        {
            if (_exceptionsStack.Count > 0 && _exceptionsStack.Peek().handlerFrame == _currentFrame)
                _exceptionsStack.Pop();
            _currentFrame.LastException = null;
            NextInstruction();
        }

        private void RaiseException(int arg)
        {
            if (arg < 0)
            {
                if (_currentFrame.LastException == null)
                    // Если в блоке Исключение была еще одна Попытка, то она затерла lastException
                    // 1С в этом случае бросает новое пустое исключение
                    throw new RuntimeException("");

                throw _currentFrame.LastException;
            }
            else
            {
                var exceptionValue = _operationStack.Pop().GetRawValue();
                if (exceptionValue is ExceptionTemplate)
                {
                    var excInfo = exceptionValue as ExceptionTemplate;
                    throw new ParametrizedRuntimeException(excInfo.Message, excInfo.Parameter);
                }
                else
                {
                    throw new RuntimeException(exceptionValue.AsString());
                }
            }
        }

        private void LineNum(int arg)
        {
            if (_currentFrame.LineNumber != arg)
            {
                _currentFrame.LineNumber = arg;
                CodeStat_LineReached();
            }

            if(MachineStopped != null && _stopManager != null && _stopManager.ShouldStopAtThisLine(_module.ModuleInfo.Origin, _currentFrame))
            {
                CreateFullCallstack();
                MachineStopped?.Invoke(this, new MachineStoppedEventArgs(MachineStopReason.Breakpoint));
            }
            
            NextInstruction();
        }

        private void CreateFullCallstack()
        {
            var result = new List<ExecutionFrameInfo>();
            /*var callstack = _callStack.ToArray();

            result.Add(FrameInfo(_module, _currentFrame));

            foreach (var executionFrame in callstack)
            {
                result.Add(FrameInfo(_module, executionFrame));
            }

            foreach (var state in _states.ToArray())
            {
                foreach (var frame in state.callStack)
                {
                    result.Add(FrameInfo(state.module, frame));
                }
            }
            */
            _fullCallstackCache = result;
        }

        private void MakeRawValue(int arg)
        {
            var value = BreakVariableLink(_operationStack.Pop());
            _operationStack.Push(value);
            NextInstruction();
        }

        private void MakeBool(int arg)
        {
            var value = _operationStack.Pop().AsBoolean();            
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void PushTmp(int arg)
        {
            var value = _operationStack.Pop();
            _currentFrame.LocalFrameStack.Push(value);
            NextInstruction();
        }

        private void PopTmp(int arg)
        {
            var tmpVal = _currentFrame.LocalFrameStack.Pop();

            if (arg == 0)
                _operationStack.Push(tmpVal);

            NextInstruction();
        }

        private void Eval(int arg)
        {
            IValue value = Evaluate(_operationStack.Pop().AsString());
            _operationStack.Push(value);
            NextInstruction();
        }

        #endregion

        #region Built-in functions

        private void Bool(int arg)
        {
            bool value = _operationStack.Pop().AsBoolean();
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void Number(int arg)
        {
            decimal value = _operationStack.Pop().AsNumber();
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void Str(int arg)
        {
            string value = _operationStack.Pop().AsString();
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void Date(int arg)
        {
            if (arg == 1)
            {
                var strDate = _operationStack.Pop().AsString();
                _operationStack.Push(ValueFactory.Parse(strDate, DataType.Date));
            }
            else if (arg >= 3 && arg <= 6)
            {
                int[] factArgs = new int[6];

                for (int i = arg - 1; i >= 0; i--)
                {
                    factArgs[i] = (int)_operationStack.Pop().AsNumber();
                }

                var date = new DateTime(
                                factArgs[0],
                                factArgs[1],
                                factArgs[2],
                                factArgs[3],
                                factArgs[4],
                                factArgs[5]);
                
                _operationStack.Push(ValueFactory.Create(date));
                       
            }
            else
            {
                throw new RuntimeException("Неверное количество параметров");
            }

            NextInstruction();
        }

        private void Type(int arg)
        {
            var typeName = _operationStack.Pop().AsString();
            var value = new TypeTypeValue(typeName);
            _operationStack.Push(value);
            NextInstruction();
        }

        private void ValType(int arg)
        {
            var value = _operationStack.Pop();
            var valueType = new TypeTypeValue(value.SystemType);
            _operationStack.Push(valueType);
            NextInstruction();
        }

        private void StrLen(int arg)
        {
            var str = _operationStack.Pop().AsString();
            _operationStack.Push(ValueFactory.Create(str.Length));
            NextInstruction();
        }

        private void TrimL(int arg)
        {
            var str = _operationStack.Pop().AsString();

            for (int i = 0; i < str.Length; i++)
            {
                if(!Char.IsWhiteSpace(str[i]))
                {
                    var trimmed = str.Substring(i);
                    _operationStack.Push(ValueFactory.Create(trimmed));
                    NextInstruction();
                    return;
                }
            }

            _operationStack.Push(ValueFactory.Create(""));
            NextInstruction();

        }

        private void TrimR(int arg)
        {
            var str = _operationStack.Pop().AsString();

            int lastIdx = str.Length-1;
            for (int i = lastIdx; i >= 0; i--)
            {
                if (!Char.IsWhiteSpace(str[i]))
                {
                    var trimmed = str.Substring(0, i+1);
                    _operationStack.Push(ValueFactory.Create(trimmed));
                    NextInstruction();
                    return;
                }
            }

            _operationStack.Push(ValueFactory.Create(""));
            NextInstruction();

        }

        private void TrimLR(int arg)
        {
            var str = _operationStack.Pop().AsString().Trim();
            _operationStack.Push(ValueFactory.Create(str));
            NextInstruction();
        }

        private void Left(int arg)
        {
            var len = (int)_operationStack.Pop().AsNumber();
            var str = _operationStack.Pop().AsString();

            if (len > str.Length)
                len = str.Length;
            else if (len < 0)
            {
                _operationStack.Push(ValueFactory.Create(""));
                NextInstruction();
                return;
            }

            _operationStack.Push(ValueFactory.Create(str.Substring(0, len)));
            NextInstruction();
        }

        private void Right(int arg)
        {
            var len = (int)_operationStack.Pop().AsNumber();
            var str = _operationStack.Pop().AsString();

            if (len > str.Length)
                len = str.Length;
            else if (len < 0)
            {
                _operationStack.Push(ValueFactory.Create(""));
                NextInstruction();
                return;
            }

            int startIdx = str.Length - len;
            _operationStack.Push(ValueFactory.Create(str.Substring(startIdx, len)));

            NextInstruction();
        }

        private void Mid(int arg)
        {
            string str;
            int start;
            int len;
            if (arg == 2)
            {
                start = (int)_operationStack.Pop().AsNumber();
                str = _operationStack.Pop().AsString();
                len = str.Length-start+1;
            }
            else
            {
                len = (int)_operationStack.Pop().AsNumber();
                start = (int)_operationStack.Pop().AsNumber();
                str = _operationStack.Pop().AsString();
            }

            if (start < 1)
                start = 1;

            if (len > str.Length || len < 0)
                len = str.Length-start+1;

            string result;

            if (start > str.Length || len == 0)
            {
                result = "";
            }
            else
            {
                result = str.Substring(start - 1, len);
            }

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }
        
        private void StrPos(int arg)
        {
            var needle = _operationStack.Pop().AsString();
            var haystack = _operationStack.Pop().AsString();

            var result = haystack.IndexOf(needle, StringComparison.Ordinal) + 1;
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void UCase(int arg)
        {
            var result = _operationStack.Pop().AsString().ToUpper();
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void LCase(int arg)
        {
            var result = _operationStack.Pop().AsString().ToLower();
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void TCase(int arg)
        {
            var argValue = _operationStack.Pop().AsString();

            char[] array = argValue.ToCharArray();
	        // Handle the first letter in the string.
            bool inWord = false;
            if (array.Length >= 1)
	        {
	            if (char.IsLetter(array[0]))
                    inWord = true;

                if(char.IsLower(array[0]))
	            {
		            array[0] = char.ToUpper(array[0]);
	            }
	        }
	        // Scan through the letters, checking for spaces.
	        // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
	        {
                if (inWord && Char.IsLetter(array[i]))
                    array[i] = Char.ToLower(array[i]);
                else if (Char.IsSeparator(array[i]) || Char.IsPunctuation(array[i]))
                    inWord = false;
                else if(!inWord && Char.IsLetter(array[i]))
                {
                    inWord = true;
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
	        }
	        
            var result = new string(array);

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void Chr(int arg)
        {
            var code = (int)_operationStack.Pop().AsNumber();

            var result = new string(new char[1] { (char)code });
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void ChrCode(int arg)
        {
            string strChar;
            int position;

            if(arg == 2)
            {
                position = (int)_operationStack.Pop().AsNumber()-1;
                strChar = _operationStack.Pop().AsString();
            }
            else if(arg == 1)
            {
                strChar = _operationStack.Pop().AsString();
                position = 0;
            }
            else
            {
                throw new WrongStackConditionException();
            }

            int result;
            if (strChar.Length == 0)
                result = 0;
            else if (position >= 0 && position < strChar.Length)
                result = (int)strChar[position];
            else
                throw RuntimeException.InvalidArgumentValue();

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EmptyStr(int arg)
        {
            var str = _operationStack.Pop().AsString();

            _operationStack.Push(ValueFactory.Create(String.IsNullOrWhiteSpace(str)));
            NextInstruction();
        }

        private void StrReplace(int arg)
        {
            var newVal = _operationStack.Pop().AsString();
            var searchVal = _operationStack.Pop().AsString();
            var sourceString = _operationStack.Pop().AsString();

            var result = sourceString.Replace(searchVal, newVal);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void StrGetLine(int arg)
        {
            var lineNumber = (int)_operationStack.Pop().AsNumber();
            var strArg = _operationStack.Pop().AsString();
            string result = "";
            if (lineNumber >= 1)
            {
                string[] subStrVals = strArg.Split(new Char[] { '\n' }, lineNumber + 1);
                result = subStrVals[lineNumber - 1];
            }

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void StrLineCount(int arg)
        {
            var strArg = _operationStack.Pop().AsString();
            int pos = 0;
            int lineCount = 1;
            while (pos >= 0 && pos < strArg.Length)
            {
                pos = strArg.IndexOf('\n', pos);
                if (pos >= 0)
                {
                    lineCount++;
                    pos++;
                }
            }

            _operationStack.Push(ValueFactory.Create(lineCount));

            NextInstruction();
        }

        private void StrEntryCount(int arg)
        {
            var what = _operationStack.Pop().AsString();
            var where = _operationStack.Pop().AsString();

            var pos = where.IndexOf(what);
            var entryCount = 0;
            while(pos >= 0)
            {
                entryCount++;
                var nextIndex = pos + what.Length;
                if (nextIndex >= where.Length)
                    break;

                pos = where.IndexOf(what, nextIndex);
            }

            _operationStack.Push(ValueFactory.Create(entryCount));

            NextInstruction();
        }

        private void Year(int arg)
        {
            var date = _operationStack.Pop().AsDate().Year;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Month(int arg)
        {
            var date = _operationStack.Pop().AsDate().Month;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Day(int arg)
        {
            var date = _operationStack.Pop().AsDate().Day;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Hour(int arg)
        {
            var date = _operationStack.Pop().AsDate().Hour;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Minute(int arg)
        {
            var date = _operationStack.Pop().AsDate().Minute;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Second(int arg)
        {
            var date = _operationStack.Pop().AsDate().Second;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void BegOfYear(int arg)
        {
            var year = _operationStack.Pop().AsDate().Year;
            _operationStack.Push(ValueFactory.Create(new DateTime(year,1,1)));
            NextInstruction();
        }

        private void BegOfMonth(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, 1);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfDay(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfHour(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfMinute(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfQuarter(int arg)
        {
            //1,4,7,10
            var date = _operationStack.Pop().AsDate();
            var month = date.Month;
            int quarterMonth;
            if (date.Month >= 1 && date.Month <= 3)
            {
                quarterMonth = 1;
            }
            else if (date.Month >= 4 && date.Month <= 6)
            {
                quarterMonth = 4;
            }
            else if (date.Month >= 7 && date.Month <= 9)
            {
                quarterMonth = 7;
            }
            else
            {
                quarterMonth = 10;
            }
            var result = new DateTime(date.Year, quarterMonth, 1);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfYear(int arg)
        {
            var year = _operationStack.Pop().AsDate().Year;
            _operationStack.Push(ValueFactory.Create(new DateTime(year, 12, DateTime.DaysInMonth(year,12), 23, 59, 59)));
            NextInstruction();
        }

        private void EndOfMonth(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfDay(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfHour(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfMinute(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfQuarter(int arg)
        {
            //1,4,7,10
            var date = _operationStack.Pop().AsDate();
            var month = date.Month;
            int quarterMonth;
            if (date.Month >= 1 && date.Month <= 3)
            {
                quarterMonth = 3;
            }
            else if (date.Month >= 4 && date.Month <= 6)
            {
                quarterMonth = 6;
            }
            else if (date.Month >= 7 && date.Month <= 9)
            {
                quarterMonth = 9;
            }
            else
            {
                quarterMonth = 12;
            }
            var result = new DateTime(date.Year, quarterMonth, DateTime.DaysInMonth(date.Year, quarterMonth), 23, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void WeekOfYear(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var cal = new System.Globalization.GregorianCalendar();

            _operationStack.Push(ValueFactory.Create(cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)));
            NextInstruction();
        }

        private void DayOfYear(int arg)
        {
            var date = _operationStack.Pop().AsDate().DayOfYear;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void DayOfWeek(int arg)
        {
            var day = (int)_operationStack.Pop().AsDate().DayOfWeek;

            if (day == 0)
            {
                day = 7;
            }

            _operationStack.Push(ValueFactory.Create(day));
            NextInstruction();
        }

        private void AddMonth(int arg)
        {
            var numToAdd = (int)_operationStack.Pop().AsNumber();
            var date = _operationStack.Pop().AsDate();
            _operationStack.Push(ValueFactory.Create(date.AddMonths(numToAdd)));
            NextInstruction();
        }

        private void CurrentDate(int arg)
        {
            _operationStack.Push(ValueFactory.Create(DateTime.Now));
            NextInstruction();
        }

        private void Integer(int arg)
        {
            var num = (int)_operationStack.Pop().AsNumber();
            _operationStack.Push(ValueFactory.Create(num));
            NextInstruction();
        }

        private void Round(int arg)
        {
            decimal num;
            int digits;
            int mode;
            if (arg == 1)
            {
                num = _operationStack.Pop().AsNumber();
                digits = 0;
                mode = 0;
            }
            else if (arg == 2)
            {
                digits = (int)_operationStack.Pop().AsNumber();
                num = _operationStack.Pop().AsNumber();
                mode = 0;
            }
            else
            {
                mode = (int)_operationStack.Pop().AsNumber();
                mode = mode == 0 ? 0 : 1;
                digits = (int)_operationStack.Pop().AsNumber();
                num = _operationStack.Pop().AsNumber();
            }

            decimal scale = (decimal)Math.Pow(10.0, digits);
            decimal scaled = Math.Abs(num) * scale;

            var director = (int)((scaled - (long)scaled) * 10 % 10);

            decimal round;
            if (director == 5)
                round = Math.Floor(scaled + mode * 0.5m * Math.Sign(digits));
            else if (director > 5)
                round = Math.Ceiling(scaled);
            else
                round = Math.Floor(scaled);
            
            decimal result;
            
            if(digits >= 0)
                result = (Math.Sign(num) * round / scale);
            else
                result = (Math.Sign(num) * round * scale);

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void Log(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Log((double) num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void Log10(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Log10((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void Sin(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Sin((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void Cos(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Cos((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void Tan(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Tan((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void ASin(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Asin((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void ACos(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Acos((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void ATan(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Atan((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }
        private void Exp(int arg)
        {
            var num = _operationStack.Pop().AsNumber();
            var result = Math.Exp((double)num);
            _operationStack.Push(ValueFactory.Create((decimal)result));
            NextInstruction();
        }

        private void Pow(int arg)
        {
            var powPower = (double)_operationStack.Pop().AsNumber();
            var powBase = (double)_operationStack.Pop().AsNumber();
            double power = Math.Pow(powBase, powPower);
            _operationStack.Push(ValueFactory.Create((decimal)power));
            NextInstruction();
        }

        private void Sqrt(int arg)
        {
            var num = (double)_operationStack.Pop().AsNumber();
            var root = Math.Sqrt(num);
            _operationStack.Push(ValueFactory.Create((decimal)root));
            NextInstruction();
        }

        private void Min(int argCount)
        {
            System.Diagnostics.Debug.Assert(argCount > 0);

            IValue min = _operationStack.Pop();
            while (--argCount > 0)
            {
                var current = _operationStack.Pop();
                if (current.CompareTo(min) < 0)
                    min = current;
            }

            _operationStack.Push(BreakVariableLink(min));

            NextInstruction();
        }

        private void Max(int argCount)
        {
            System.Diagnostics.Debug.Assert(argCount > 0);

            IValue max = _operationStack.Pop();
            while (--argCount > 0)
            {
                var current = _operationStack.Pop();
                if (current.CompareTo(max) > 0)
                    max = current;
            }

            _operationStack.Push(BreakVariableLink(max));
            NextInstruction();
        }

        private void Format(int arg)
        {
            var formatString = _operationStack.Pop().AsString();
            var valueToFormat = _operationStack.Pop();

            var formatted = ValueFormatter.Format(valueToFormat, formatString);

            _operationStack.Push(ValueFactory.Create(formatted));
            NextInstruction();

        }

        private void ExceptionInfo(int arg)
        {
            if (_currentFrame.LastException != null)
            {
                ExceptionInfoContext excInfo;
                if (_currentFrame.LastException is ParametrizedRuntimeException)
                    excInfo = new ExceptionInfoContext((ParametrizedRuntimeException)_currentFrame.LastException);
                else
                    excInfo = new ExceptionInfoContext(_currentFrame.LastException);

                _operationStack.Push(ValueFactory.Create(excInfo));
            }
            else
            {
                _operationStack.Push(ValueFactory.Create());
            }
            NextInstruction();
        }
        
        private void ExceptionDescr(int arg)
        {
            if (_currentFrame.LastException != null)
            {
                var excInfo = new ExceptionInfoContext(_currentFrame.LastException);
                _operationStack.Push(ValueFactory.Create(excInfo.MessageWithoutCodeFragment));
            }
            else
            {
                _operationStack.Push(ValueFactory.Create(""));
            }
            NextInstruction();
        }

        private void ModuleInfo(int arg)
        {
            var currentScript = this.CurrentScript;
            if (currentScript != null)
            {
                _operationStack.Push(currentScript);
            }
            else
            {
                _operationStack.Push(ValueFactory.Create());
            }
            NextInstruction();
        }

        #endregion

        #endregion

        private LoadedModule CompileExpressionModule(string expression)
        {
            var ctx = ExtractCompilerContext();

            ICodeSource stringSource = new StringBasedSource(expression);
            var parser = new Parser();
            parser.Code = stringSource.Code;
            var compiler = new Compiler.Compiler();
            ctx.PushScope(new SymbolScope()); // скоуп выражения
            var modImg = compiler.CompileExpression(parser, ctx);
            modImg.ModuleInfo = new ModuleInformation();
            modImg.ModuleInfo.Origin = "<expression>";
            modImg.ModuleInfo.ModuleName = "<expression>";
            var code = new LoadedModule(modImg);
            return code;
        }

        private CompilerContext ExtractCompilerContext()
        {
            var ctx = new CompilerContext();
            foreach (var scope in _scopes)
            {
                var symbolScope = new SymbolScope();
                foreach (var methodInfo in scope.Methods)
                {
                    symbolScope.DefineMethod(methodInfo);
                }
                foreach (var variable in scope.Variables)
                {
                    symbolScope.DefineVariable(variable.Name);
                }

                ctx.PushScope(symbolScope);
            }

            var locals = new SymbolScope();
            foreach (var variable in _currentFrame.Locals)
            {
                locals.DefineVariable(variable.Name);
            }

            ctx.PushScope(locals);
            return ctx;
        }

        private void NextInstruction()
        {
            _currentFrame.InstructionPointer++;
        }

        private IValue BreakVariableLink(IValue value)
        {
            return value.GetRawValue();
        }

        public IList<ExecutionFrameInfo> GetExecutionFrames()
        {
            return _fullCallstackCache;
        }

        public IList<IVariable> GetFrameLocals(int frameId)
        {
            System.Diagnostics.Debug.Assert(_fullCallstackCache != null);
            if (frameId < 0 || frameId >= _fullCallstackCache.Count)
                return new IVariable[0];

            var frame = _fullCallstackCache[frameId];
            return frame.FrameObject.Locals;
        }

        private ExecutionFrameInfo FrameInfo(LoadedModule module, ExecutionFrame frame)
        {
            return new ExecutionFrameInfo()
            {
                LineNumber = frame.LineNumber,
                MethodName = frame.MethodName,
                Source = module.ModuleInfo.Origin,
                FrameObject = frame
            };
        }

      
    }
}
