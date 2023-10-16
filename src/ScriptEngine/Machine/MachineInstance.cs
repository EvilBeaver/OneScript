/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OneScript.Commons;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Language;
using OneScript.Sources;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Compiler;

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
        private LruCache<string, StackRuntimeModule> _executeModuleCache = new LruCache<string, StackRuntimeModule>(64);

        private StackRuntimeModule _module;
        private ICodeStatCollector _codeStatCollector;
        private MachineStopManager _stopManager;
        
        private ExecutionContext _mem;

        // для отладчика.
        // актуален в момент останова машины
        private IList<ExecutionFrameInfo> _fullCallstackCache;
        private ScriptInformationContext _debugInfo;

        private MachineInstance() 
        {
            InitCommands();
            Reset();
        }

        public event EventHandler<MachineStoppedEventArgs> MachineStopped;

        public void AttachContext(IAttachableContext context)
        {
            _scopes.Add(CreateModuleScope(context));
        }

        private Scope CreateModuleScope(IAttachableContext context)
        {
            IVariable[] vars;
            BslMethodInfo[] methods;
            context.OnAttach(out vars, out methods);
            var scope = new Scope()
            {
                Variables = vars,
                Methods = methods,
                Instance = context
            };
            return scope;
        }

        public void ContextsAttached()
        {
            // module scope
            _scopes.Add(default(Scope));
        }

        public ExecutionContext Memory => _mem;

        public ITypeManager TypeManager => _mem?.TypeManager;
        
        public IGlobalsManager Globals => _mem?.GlobalInstances;
        
        public RuntimeEnvironment Environment => _mem?.GlobalNamespace;

        public void SetMemory(ExecutionContext memory)
        {
            Cleanup();
            foreach (var item in memory.GlobalNamespace.AttachedContexts.Select(x=>x.Instance))
            {
                AttachContext(item);
            }
            ContextsAttached();

            _mem = memory;
            _codeStatCollector = _mem.Services.TryResolve<ICodeStatCollector>();
        }
        
        internal MachineStoredState SaveState()
        {
            return new MachineStoredState()
            {
                Scopes = _scopes,
                ExceptionsStack = _exceptionsStack,
                CallStack = _callStack,
                OperationStack = _operationStack,
                StopManager = _stopManager,
                Memory = _mem
            };
        }

        internal void RestoreState(MachineStoredState state)
        {
            Reset();
            _scopes = state.Scopes;
            _operationStack = state.OperationStack;
            _callStack = state.CallStack;
            _exceptionsStack = state.ExceptionsStack;
            _stopManager = state.StopManager;
            _mem = state.Memory; 
            
            SetFrame(_callStack.Peek());
        } 

        internal Task<IValue> ExecuteMethodAsync(IRunnable sdo, int methodIndex, IValue[] arguments)
        {
            var state = SaveState();
            Func<IValue> callAction = () =>
            {
                var m = MachineInstance.Current;
                m.Reset();
                m._scopes = state.Scopes; // память сохраняется, стеки с чистого листа
                m._mem = state.Memory;
                return m.ExecuteMethod(sdo, methodIndex, arguments);
            };

            _currentFrame = new ExecutionFrame()
            {
                InstructionPointer = -1
            };

            var asyncTask = Task.Run(callAction);
                
            asyncTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception.Flatten();
                }

                var m = MachineInstance.Current;
                m.RestoreState(state);
                m.ExecuteCode();
            });

            return asyncTask;
        }
        
        public bool IsRunning => _callStack.Count != 0;

        [Obsolete]
        internal IValue ExecuteMethod(IRunnable sdo, int methodIndex, IValue[] arguments)
        {
            var methodInfo = (MachineMethodInfo)sdo.Module.Methods[methodIndex];
            return ExecuteMethod(sdo, methodInfo, arguments);
        }
        
        internal IValue ExecuteMethod(IRunnable sdo, MachineMethodInfo methodInfo, IValue[] arguments)
        {
            var module = sdo.Module as StackRuntimeModule;
            Debug.Assert(module != null);
            var frame = new ExecutionFrame
            {
                Module = module,
                ModuleScope = CreateModuleScope(sdo),
                ModuleLoadIndex = module.LoadAddress,
                IsReentrantCall = true,
            };
            SetExecutionFrame(frame, methodInfo, arguments);

            ExecuteCode();

            IValue methodResult = null;
            if (methodInfo.IsFunction())
            {
                methodResult = _operationStack.Pop();
            }

            // Этот Pop связан с методом Return. 
            // Если идет возврат из вложенного вызова, то Pop делается здесь, а не в Return,
            // т.к. мы должны выйти из MainCommandLoop и вернуться в предыдущий цикл машины
            //
            // P.S. it's fuckin spaghetti (
            if (_callStack.Count > 1)
                PopFrame();

            return methodResult;
        }

        private void SetExecutionFrame(ExecutionFrame frame, MachineMethodInfo methodInfo, IValue[] argValues)
        {
            var methDescr = methodInfo.GetRuntimeMethod();
            frame.MethodName = methodInfo.Name;
            frame.InstructionPointer = methDescr.EntryPoint;

            var parameters = methodInfo.GetBslParameters();
            var variables = methDescr.LocalVariables;
            var locals = new IVariable[variables.Length];
            int i = 0;
            for (; i < argValues.Length; i++)
            {
                var paramDef = parameters[i];
                var argValue = argValues[i];
                if (argValue is IVariable argVar)
                {
                    if (paramDef.ExplicitByVal)
                    {
                        locals[i] = Variable.Create(argVar.Value, variables[i]);
                    }
                    else
                    {
                        // TODO: Alias ?
                        locals[i] = Variable.CreateReference(argVar, variables[i]);
                    }
                }
                else if (argValue == null || argValue.IsSkippedArgument())
                {
                    var value = paramDef.HasDefaultValue ? (IValue)paramDef.DefaultValue : ValueFactory.Create();
                    locals[i] = Variable.Create(value, variables[i]);
                }
                else
                {
                    locals[i] = Variable.Create(argValue, variables[i]);
                }
            }
            for (; i < parameters.Length; i++)
            {
                var paramDef = parameters[i];
                var value = paramDef.HasDefaultValue ? (IValue)paramDef.DefaultValue : ValueFactory.Create();
                locals[i] = Variable.Create(value, variables[i]);
            }
            for (; i < locals.Length; i++)
            {
                locals[i] = Variable.Create(ValueFactory.Create(), variables[i]);
            }

            frame.Locals = locals;
            PushFrame(frame);
        }

        #region Debug protocol methods

        public void SetDebugMode(IBreakpointManager breakpointManager)
        {
            if (_stopManager == null)
                _stopManager = new MachineStopManager(this, breakpointManager);
        }
        
        public void UnsetDebugMode()
        {
            if (_stopManager != null)
                PrepareDebugContinuation();

            _stopManager = null;
        }

        public void StepOver()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

            _stopManager.StepOver(_currentFrame);
        }

        public void StepIn()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

            _stopManager.StepIn();
        }

        public void StepOut()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

            _stopManager.StepOut(_currentFrame);
        }

        public void PrepareDebugContinuation()
        {
            if (_stopManager == null)
                throw new InvalidOperationException("Machine is not in debug mode");

            _stopManager.Continue();
        }

        public IValue Evaluate(string expression)
        {
            var code = CompileCached(expression, CompileExpressionModule);

            var localScope = new Scope
            {
                Instance = new UserScriptContextInstance(code),
                Methods = new BslMethodInfo[0],
                Variables = _currentFrame.Locals
            };
            _scopes.Add(localScope);

            var frame = new ExecutionFrame
            {
                MethodName = code.Source.Name,
                Module = code,
                ModuleScope = localScope,
                ModuleLoadIndex = _scopes.Count - 1,
                Locals = new IVariable[0],
                InstructionPointer = 0,
            };

            try
            {
                PushFrame(frame);
                MainCommandLoop();
            }
            finally
            {
                PopFrame();
                _scopes.RemoveAt(_scopes.Count - 1);
            }

            return _operationStack.Pop();
        }

        public IValue EvaluateInFrame(string expression, int frameId)
        {
            System.Diagnostics.Debug.Assert(_fullCallstackCache != null);
            if (frameId < 0 || frameId >= _fullCallstackCache.Count)
                throw new ScriptException("Wrong stackframe");

            ExecutionFrame selectedFrame = _fullCallstackCache[frameId].FrameObject;

            MachineInstance currentMachine;
            MachineInstance runner = new MachineInstance
            {
                _mem = this._mem,
                _scopes = new List<Scope>(this._scopes.GetRange(0, selectedFrame.ModuleLoadIndex + 1)),
                _debugInfo = CurrentScript
            };
            currentMachine = Current;
            SetCurrentMachineInstance(runner);

            runner.SetFrame(selectedFrame);

            ExecutionFrame frame;
            try
            {
                var code = runner.CompileExpressionModule(expression);

                var localScope = new Scope
                {
                    Instance = new UserScriptContextInstance(code),
                    Methods = new BslMethodInfo[0],
                    Variables = selectedFrame.Locals
                };
                runner._scopes.Add(localScope);

                frame = new ExecutionFrame
                {
                    MethodName = code.Source.Name,
                    Module = code,
                    ModuleScope = localScope,
                    ModuleLoadIndex = runner._scopes.Count - 1,
                    Locals = new IVariable[0],
                    InstructionPointer = 0,
                };
            }
            catch
            {
                SetCurrentMachineInstance(currentMachine);
                throw;
            }

            try
            {
                runner.PushFrame(frame);
                runner.MainCommandLoop();
            }
            finally
            {
                SetCurrentMachineInstance(currentMachine);
            }

            return runner._operationStack.Pop().GetRawValue();
        }

        private StackRuntimeModule CompileCached(string code, Func<string, StackRuntimeModule> compile)
        {
            var cacheKey = HashCode.Combine(code, _module.Source.Location, _currentFrame.ToString()).ToString("X8");
            return _executeModuleCache.GetOrAdd(cacheKey, _ => compile(code));
        }
        
        #endregion

        /// <summary>
        /// Обработчик событий, генерируемых классами прикладной логики.
        /// </summary>
        public IEventProcessor EventProcessor { get; set; }
        
        private ScriptInformationContext CurrentScript
        {
            get
            {
                if (_module.Source != null)
                    return new ScriptInformationContext(_module.Source);
                else
                    return null;
            }
        }

        internal void Cleanup()
        {
            Reset();
        }
        
        private void PushFrame(ExecutionFrame frame)
        {
            _callStack.Push(frame);
            SetFrame(frame);
        }

        private void PopFrame()
        {
            _callStack.Pop();
            SetFrame(_callStack.Peek());
        }

        private void SetFrame(ExecutionFrame frame)
        {
            _module = frame.Module;
            _scopes[frame.ModuleLoadIndex] = frame.ModuleScope;
            _currentFrame = frame;
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
            _module = null;
            _currentFrame = null;
            _mem = null;
        }

        private void PrepareCodeStatisticsData(StackRuntimeModule _module)
        {
            if (_codeStatCollector == null
                || _codeStatCollector.IsPrepared(_module.Source.Location))
            {
                return;
            }
            
            foreach (var method in _module.Methods
                .Cast<MachineMethodInfo>())
            {
                var instructionPointer = method.GetRuntimeMethod().EntryPoint;
                while (instructionPointer < _module.Code.Count)
                {
                    if (_module.Code[instructionPointer].Code == OperationCode.LineNum)
                    {
                        var entry = new CodeStatEntry(
                            _module.Source.Location,
                            method.Name,
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
            _codeStatCollector.MarkPrepared(_module.Source.Location);
        }

        private void ExecuteCode()
        {
            PrepareCodeStatisticsData(_module);

            while (true)
            {
                try
                {
                    MainCommandLoop();
                    break;
                }
                catch (ScriptException exc)
                {
                    SetScriptExceptionSource(exc);

                    if (ShouldRethrowException(exc))
                        throw;
                }
            }
        }

        private bool ShouldRethrowException(ScriptException exc)
        {
            if (_exceptionsStack.Count == 0)
            {
                return true;
            }

            var callStackFrames = exc.RuntimeSpecificInfo as IList<ExecutionFrameInfo>;

            if (callStackFrames == null)
            {
                CreateFullCallstack();
                callStackFrames = new List<ExecutionFrameInfo>(_fullCallstackCache);
                exc.RuntimeSpecificInfo = callStackFrames;
            }

            var handler = _exceptionsStack.Pop();

            // Раскрутка стека вызовов
            while (_currentFrame != handler.HandlerFrame)
            {
                if (_currentFrame.IsReentrantCall)
                {
                    _exceptionsStack.Push(handler);
                    PopFrame();
                    return true;
                }

                PopFrame();
            }

            _currentFrame.InstructionPointer = handler.HandlerAddress;
            _currentFrame.LastException = exc;

            // При возникновении исключения посредине выражения
            // некому почистить стек операндов.
            // Сделаем это
            while (_operationStack.Count > handler.StackSize)
                _operationStack.Pop();

            return false;
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
                       && _currentFrame.InstructionPointer < _module.Code.Count)
                {
                    var command = _module.Code[_currentFrame.InstructionPointer];
                    _commands[(int) command.Code](command.Argument);
                }
            }
            catch (ScriptInterruptionException)
            {
                throw;
            }
            catch (ScriptException exc)
            {
                exc.SetPositionIfEmpty(GetPositionInfo());

                throw;
            }
            catch (Exception exc)
            {
                var excWrapper = new ExternalSystemException(exc);
                SetScriptExceptionSource(excWrapper);
                throw excWrapper;
            }
        }

        private ErrorPositionInfo GetPositionInfo()
        {
            var epi = new ErrorPositionInfo();
            epi.LineNumber = _currentFrame.LineNumber;
            if (_module.Source != null && epi.LineNumber > 0)
            {
                epi.ModuleName = _module.Source.Name;
                epi.Code = _module.Source.GetCodeLine(epi.LineNumber) ?? "<исходный код недоступен>";
            }
            else
            {
                epi.ModuleName = "<имя модуля недоступно>";
                epi.Code = "<исходный код недоступен>";
            }
            return  epi;
        }

        private void SetScriptExceptionSource(ScriptException exc)
        {
            exc.SetPositionIfEmpty(GetPositionInfo());
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
                NewFunc,
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
                Execute,
                AddHandler,
                RemoveHandler,
                ExitTry,

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
                BegOfWeek,
                BegOfYear,
                BegOfMonth,
                BegOfDay,
                BegOfHour,
                BegOfMinute,
                BegOfQuarter,
                EndOfWeek,
                EndOfYear,
                EndOfMonth,
                EndOfDay,
                EndOfHour,
                EndOfMinute,
                EndOfQuarter,
                WeekOfYear,
                DayOfYear,
                DayOfWeek,
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
            var scope = _scopes[vm.ScopeNumber];
            _operationStack.Push(scope.Variables[vm.MemberNumber]);
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
            var scope = _scopes[vm.ScopeNumber];
            var reference = Variable.CreateContextPropertyReference(scope.Instance, vm.MemberNumber, "$stackvar");
            _operationStack.Push(reference);
            NextInstruction();
        }

        private void LoadVar(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ScopeNumber];
            scope.Variables[vm.MemberNumber].Value = PopRawValue();
            NextInstruction();
        }

        private void LoadLoc(int arg)
        {
            _currentFrame.Locals[arg].Value = PopRawValue();
            NextInstruction();
        }

        private void AssignRef(int arg)
        {
            var value = PopRawValue();

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
            _operationStack.Push(ValueFactory.Add(op1.GetRawValue(), op2.GetRawValue()));
            NextInstruction();
        }

        private void Sub(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Sub(op1.GetRawValue(), op2.GetRawValue()));
            NextInstruction();
        }

        private void Mul(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Mul(op1.GetRawValue(), op2.GetRawValue()));
            NextInstruction();
        }

        private void Div(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Div(op1.GetRawValue(), op2.GetRawValue()));
            NextInstruction();
        }

        private void Mod(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Mod(op1.GetRawValue(), op2.GetRawValue()));
            NextInstruction();
        }

        private void Neg(int arg)
        {
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Neg(op1.GetRawValue()));
            NextInstruction();
        }

        private void Equals(int arg)
        {
            var op2 = _operationStack.Pop().GetRawValue();
            var op1 = _operationStack.Pop().GetRawValue();
            _operationStack.Push(ValueFactory.Create(op1.Equals(op2)));
            NextInstruction();
        }

        private void Less(int arg)
        {
            var op2 = _operationStack.Pop().GetRawValue();
            var op1 = _operationStack.Pop().GetRawValue();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) < 0));
            NextInstruction();
        }

        private void Greater(int arg)
        {
            var op2 = _operationStack.Pop().GetRawValue();
            var op1 = _operationStack.Pop().GetRawValue();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) > 0));
            NextInstruction();
        }

        private void LessOrEqual(int arg)
        {
            var op2 = _operationStack.Pop().GetRawValue();
            var op1 = _operationStack.Pop().GetRawValue();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) <= 0));
            NextInstruction();
        }

        private void GreaterOrEqual(int arg)
        {
            var op2 = _operationStack.Pop().GetRawValue();
            var op1 = _operationStack.Pop().GetRawValue();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) >= 0));
            NextInstruction();
        }

        private void NotEqual(int arg)
        {
            var op2 = _operationStack.Pop().GetRawValue();
            var op1 = _operationStack.Pop().GetRawValue();
            _operationStack.Push(ValueFactory.Create(!op1.Equals(op2)));
            NextInstruction();
        }

        private void Not(int arg)
        {
            var op1 = _operationStack.Pop().GetRawValue();
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

        private IValue[] PopArguments()
        {
            int argCount = (int)_operationStack.Pop().AsNumber();
            IValue[] args = new IValue[argCount];

            for (--argCount; argCount >= 0; --argCount)
            {
                args[argCount] = _operationStack.Pop();
            }
            return args;
        }

        private bool MethodCallImpl(int arg, bool asFunc)
        {
            var methodRef = _module.MethodRefs[arg];
            var scope = _scopes[methodRef.ScopeNumber];
            var methodSignature = scope.Methods[methodRef.MemberNumber];

            IValue[] argValues = PopArguments();

            var definedParameters = methodSignature.GetParameters();
            bool needsDiscarding;

            if (scope.Instance == this.TopScope.Instance) // local call
            {
                var sdo = scope.Instance as ScriptDrivenObject;
                System.Diagnostics.Debug.Assert(sdo != null);

                if (sdo.MethodDefinedInScript(methodRef.MemberNumber))
                {
                    // заранее переведем указатель на адрес возврата. В опкоде Return инкремента нет.
                    NextInstruction();

                    var methodInfo = (MachineMethodInfo)_module.Methods[sdo.GetMethodDescriptorIndex(methodRef.MemberNumber)];
                    var frame = new ExecutionFrame
                    {
                        Module = _module,
                        ModuleScope = TopScope,
                        ModuleLoadIndex = _scopes.Count - 1,
                    };
                    SetExecutionFrame(frame, methodInfo, argValues);

                    needsDiscarding = methodSignature.IsFunction() && !asFunc;
                }
                else
                {
                    needsDiscarding = _currentFrame.DiscardReturnValue;
                    CallContext(scope.Instance, methodRef.MemberNumber, definedParameters, argValues, asFunc);
                }
            }
            else
            {
                // при вызове библиотечного метода (из другого scope)
                // статус вызова текущего frames не должен изменяться.
                //
                needsDiscarding = _currentFrame.DiscardReturnValue;
                CallContext(scope.Instance, methodRef.MemberNumber, definedParameters, argValues, asFunc);
            }

            return needsDiscarding;
        }

        private void CallContext(IRuntimeContextInstance instance, int index, ParameterInfo[] definedParameters, IValue[] argValues, bool asFunc)
        {
            IValue[] realArgs;
            if (instance.DynamicMethodSignatures)
            {
                realArgs = argValues;
            }
            else
            {
                realArgs = new IValue[definedParameters.Length];
                var skippedArg = BslSkippedParameterValue.Instance;
                int i = 0;
                for (; i < argValues.Length; i++)
                {
                    realArgs[i] = argValues[i];
                }
                for (; i < realArgs.Length; i++)
                {
                    realArgs[i] = skippedArg;
                }
            }
 
            if (asFunc)
            {
                instance.CallAsFunction(index, realArgs, out IValue retVal);
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
            
            var context = objIValue.AsObject();
            var propName = _module.Constants[arg].AsString();
            var propNum = context.GetPropertyNumber(propName);

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

            if (!context.DynamicMethodSignatures && context.GetMethodInfo(methodId).ReturnType == typeof(void))
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
            var factArgs = PopArguments();
            var argCount = factArgs.Length;
 
            var objIValue = _operationStack.Pop();
            context = objIValue.AsObject();
            var methodName = _module.Constants[arg].AsString();
            methodId = context.GetMethodNumber(methodName);
            
            if (context.DynamicMethodSignatures)
            {
                argValues = new IValue[argCount];
                for (int i = 0; i < argCount; i++)
                {
                    var argValue = factArgs[i];
                    if (!argValue.IsSkippedArgument())
                    {
                        argValues[i] = argValue;
                    }
                }
            }
            else
            {
                var methodInfo = context.GetMethodInfo(methodId);
                var methodParams = methodInfo.GetParameters();

                if (argCount > methodParams.Length)
                    throw RuntimeException.TooManyArgumentsPassed();

                argValues = new IValue[methodParams.Length];
                int i = 0;
                for (; i < argCount; i++)
                {
                    var argValue = factArgs[i];
                    if (!argValue.IsSkippedArgument())
                    {
                        if (methodParams[i].IsByRef())
                        {
                            argValues[i] = argValue is IVariable? argValue : Variable.Create(argValue, "");
                        }
                        else
                            argValues[i] = argValue.GetRawValue();
                    }
                    else if(!methodParams[i].HasDefaultValue)
                        throw RuntimeException.MissedArgument();
                }
                for (; i < methodParams.Length; i++)
                {
                    if (!methodParams[i].HasDefaultValue)
                        throw RuntimeException.TooFewArgumentsPassed();
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
            var index = PopRawValue();
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

            while(_exceptionsStack.Count > 0 && _exceptionsStack.Peek().HandlerFrame == _currentFrame)
            {
                _exceptionsStack.Pop();
            }

            if (_currentFrame.IsReentrantCall)
                _currentFrame.InstructionPointer = -1;
            else
            {
                PopFrame();
                if(DebugStepInProgress())
                    EmitStopEventIfNecessary();
            }
        }

        private bool DebugStepInProgress()
        {
            if (_stopManager == null)
                return false;

            return _stopManager.CurrentState == DebugState.SteppingOut || _stopManager.CurrentState == DebugState.SteppingOver;
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
                if(!argValue.IsSkippedArgument())
                    argValues[i] = argValue.GetRawValue();
            }

            var typeName = _operationStack.Pop().AsString();
            if (!TypeManager.TryGetType(typeName, out var type))
            {
                throw RuntimeException.ConstructorNotFound(typeName);
            }
            
            // TODO убрать cast после рефакторинга ITypeFactory
            var factory = (TypeFactory)TypeManager.GetFactoryFor(type);
            var context = new TypeActivationContext
            {
                TypeName = typeName,
                TypeManager = _mem.TypeManager,
                Services = _mem.Services
            };
            
            var instance = (IValue)factory.Activate(context, argValues);
            _operationStack.Push(instance);
            NextInstruction();
        }

        private void PushIterator(int arg)
        {
            var collection = _operationStack.Pop().GetRawValue();
            // TODO: возможно, можем избавиться от вызова AsObject и сразу проверять тип collection на ICollectionContext
            // Нужно проверить, как ведет себя 1С, если в Для Каждого кидаем НеОбъект. Будет ли исключение приведения к объекту?
            // Если "Значение не явл. значением объектного типа" 1С не выдает, а всегда выдает "Итератор не определен"
            // то можем удалить данный вызов .AsObject, т.к. он здесь оставлен только ради исключения ValueIsNotObjectException
            var rci = collection.AsObject();
            if (rci is ICollectionContext<IValue> context)
            {
                var iterator = context.GetManagedIterator();
                _currentFrame.LocalFrameStack.Push(iterator);
                NextInstruction();

            }
            else
            {
                throw RuntimeException.IteratorIsNotDefined();
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
            info.HandlerAddress = exceptBlockAddress;
            info.HandlerFrame = _currentFrame;
            info.StackSize = _operationStack.Count;

            _exceptionsStack.Push(info);
            NextInstruction();
        }

        private void EndTry(int arg)
        {
            if (_exceptionsStack.Count > 0 && _exceptionsStack.Peek().HandlerFrame == _currentFrame)
                _exceptionsStack.Pop();
            _currentFrame.LastException = null;
            NextInstruction();
        }

        private void RaiseException(int arg)
        {
            if (arg < 0)
            {
                if (_currentFrame.LastException == null)
                {
                    // Если в блоке Исключение была еще одна Попытка, то она затерла lastException
                    // 1С в этом случае бросает новое пустое исключение
                    //throw new RuntimeException("");
                    throw new RuntimeException("");
                }

                throw _currentFrame.LastException;
            }
            else
            {
                var exceptionValue = _operationStack.Pop().GetRawValue();
                if (exceptionValue is ExceptionInfoContext { IsErrorTemplate: true } excInfo)
                {
                    throw new ParametrizedRuntimeException(excInfo.Description, excInfo.Parameters);
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

            EmitStopEventIfNecessary();

            NextInstruction();
        }

        private void EmitStopEventIfNecessary()
        {
            if (MachineStopped != null && _stopManager != null && _stopManager.ShouldStopAtThisLine(_module.Source.Location, _currentFrame))
            {
                CreateFullCallstack();
                MachineStopped?.Invoke(this, new MachineStoppedEventArgs(_stopManager.LastStopReason));
            }
        }

        private void CreateFullCallstack()
        {
            var result = _callStack.Select(x => FrameInfo(x.Module, x)).ToList();
            _fullCallstackCache = result;
        }

        private void MakeRawValue(int arg)
        {
            var value = PopRawValue();
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

        private void Execute(int arg)
        {
            var code = _operationStack.Pop().AsString();
            var module = CompileCached(code, CompileExecutionBatchModule);
            if (module.Methods.Count() == 0)
            {
                NextInstruction();
                return;
            }

            var localScope = new Scope
            {
                Instance = new UserScriptContextInstance(module),
                Methods = new BslMethodInfo[0],
                Variables = _currentFrame.Locals
            };
            _scopes.Add(localScope);

            var mi = (MachineMethodInfo)module.Methods[0];
            var method = mi.GetRuntimeMethod();
            var frame = new ExecutionFrame
            {
                Module = module,
                MethodName = mi.Name,
                ModuleScope = localScope,
                ModuleLoadIndex = _scopes.Count - 1,
                Locals = new IVariable[method.LocalVariables.Length],
                InstructionPointer = 0
            };
            var locals = frame.Locals;
            for (int i = 0; i < locals.Length; i++)
            {
                locals[i] = Variable.Create(ValueFactory.Create(), method.LocalVariables[i]);
            }

            PushFrame(frame);
            try
            {
                ExecuteCode();
                PopFrame();
            }
            finally
            {
                _scopes.RemoveAt(_scopes.Count - 1);
            }

            NextInstruction();
        }

        private void Eval(int arg)
        {
            IValue value = Evaluate(_operationStack.Pop().AsString());
            _operationStack.Push(value);
            NextInstruction();
        }

        private void AddHandler(int arg)
        {
            PrepareHandlerOperationArgs(
                arg == 0,
                out var handlerMethod,
                out var handlerTarget,
                out var eventName,
                out var eventSource);
            
            EventProcessor?.AddHandler(eventSource, eventName, handlerTarget, handlerMethod);
            
            NextInstruction();
        }
        
        private void RemoveHandler(int arg)
        {
            PrepareHandlerOperationArgs(
                arg == 0,
                out var handlerMethod,
                out var handlerTarget,
                out var eventName,
                out var eventSource);
            
            EventProcessor?.RemoveHandler(eventSource, eventName, handlerTarget, handlerMethod);
            
            NextInstruction();
        }

        private void PrepareHandlerOperationArgs(bool useExportMode,
            out string handlerMethod,
            out IRuntimeContextInstance handlerTarget,
            out string eventName,
            out IRuntimeContextInstance eventSource)
        {
            if (useExportMode)
            {
                handlerMethod = _operationStack.Pop().AsString();
                handlerTarget = _operationStack.Pop().AsObject();
                eventName = _operationStack.Pop().AsString();
                eventSource = _operationStack.Pop().AsObject();
                
                // Выбросит исключение, если не найден такой метод
                handlerTarget.GetMethodNumber(handlerMethod);
            }
            else
            {
                handlerMethod = _operationStack.Pop().AsString();
                handlerTarget = TopScope.Instance;
                eventName = _operationStack.Pop().AsString();
                eventSource = _operationStack.Pop().AsObject();
            }
        }

        private void ExitTry(int arg)
        {
            while (arg-- > 0)
                _exceptionsStack.Pop();
            
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
            var type = TypeManager.GetTypeByName(typeName);
            var value = new BslTypeValue(type);
            _operationStack.Push(value);
            NextInstruction();
        }

        private void ValType(int arg)
        {
            var value = _operationStack.Pop();
            var valueType = new BslTypeValue(value.SystemType);
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

            if (start+len > str.Length || len < 0)
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

            var result = (code >= 0 && code < 65536) ? new String((char)code, 1) : String.Empty;
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

            int result = (position >= 0 && position < strChar.Length) ? strChar[position] : -1;

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

        private DateTime DropTimeFraction(in DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }
        
        private void BegOfWeek(int arg)
        {
            var date = DropTimeFraction(_operationStack.Pop().AsDate());
            
            var numDayOfWeek = (int)date.DayOfWeek;
            if (numDayOfWeek == 0)
            {
                numDayOfWeek = 7;
            }

            var desiredDate = date.AddDays(-(numDayOfWeek - 1));
            _operationStack.Push(ValueFactory.Create(desiredDate));
            
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

        private void EndOfWeek(int arg)
        {
            var date = DropTimeFraction(_operationStack.Pop().AsDate());
            
            var numDayOfWeek = (int)date.DayOfWeek;
            if (numDayOfWeek == 0)
            {
                numDayOfWeek = 7;
            }

            var desiredDate = date.AddDays(7 - numDayOfWeek);
            _operationStack.Push(ValueFactory.Create(new DateTime(desiredDate.Year, desiredDate.Month, desiredDate.Day, 23, 59, 59)));
            
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
            var date = DateTime.Now;
            date = date.AddTicks(-(date.Ticks % TimeSpan.TicksPerSecond));
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Integer(int arg)
        {
            var num = Math.Truncate(_operationStack.Pop().AsNumber());
            _operationStack.Push(ValueFactory.Create(num));
            NextInstruction();
        }

        private void Round(int arg)
        {
            decimal num;
            int digits;
            int mode = 1; // по умолчанию Окр15как20
            if (arg == 1)
            {
                num = _operationStack.Pop().AsNumber();
                digits = 0;
            }
            else if (arg == 2)
            {
                digits = (int)_operationStack.Pop().AsNumber();
                num = _operationStack.Pop().AsNumber();
            }
            else
            {
                mode = (int)_operationStack.Pop().AsNumber();
                mode = mode == 0 ? 0 : 1;
                digits = (int)_operationStack.Pop().AsNumber();
                num = _operationStack.Pop().AsNumber();
            }

            decimal result;
            if (digits >= 0)
            {
                result = Math.Round(num, digits, MidpointRounding.AwayFromZero);
                if (mode == 0)
                {
                    int scale = (int)Math.Pow(10, digits);
                    // для.Net Core 3+, 5+ можно использовать MidpointRounding.ToZero
                    var diff = (result - num) * scale;
                    if (diff == 0.5m)
                        result -= 1m / scale;
                    else if (diff == -0.5m)
                        result += 1m / scale;
                }
            }
            else
            {
                int scale = (int)Math.Pow(10, -digits);
                num /= scale;
                result = Math.Round(num, MidpointRounding.AwayFromZero);
                if (mode == 0)
                {
                    var diff = result - num;
                    if (diff == 0.5m)
                        result -= 1m;
                    else if (diff == -0.5m)
                        result += 1m;
                }
                result *= scale;
            }

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
            var powPower = _operationStack.Pop().AsNumber();
            var powBase = _operationStack.Pop().AsNumber();

            int exp = (int)powPower;
            decimal result;
            if (exp >= 0 && exp == powPower)
                result = PowInt(powBase, (uint)exp);
            else
                result = (decimal)Math.Pow((double)powBase, (double)powPower);

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private decimal PowInt(decimal bas, uint exp)
        {
            decimal pow = 1;

            while (true)
            {
                if ((exp & 1) == 1) pow *= bas;
                exp >>= 1;
                if (exp == 0) break;
                bas *= bas;
            }

            return pow;
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

            IValue min = _operationStack.Pop().GetRawValue();
            while (--argCount > 0)
            {
                var current = _operationStack.Pop().GetRawValue();
                if (current.CompareTo(min) < 0)
                    min = current;
            }

            _operationStack.Push(min.GetRawValue());

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

            _operationStack.Push(max.GetRawValue());
            NextInstruction();
        }

        private void Format(int arg)
        {
            var formatString = _operationStack.Pop().AsString();
            var valueToFormat = _operationStack.Pop().GetRawValue();

            var formatted = ValueFormatter.Format((BslValue)valueToFormat, formatString);

            _operationStack.Push(ValueFactory.Create(formatted));
            NextInstruction();

        }

        private void ExceptionInfo(int arg)
        {
            if (_currentFrame.LastException != null)
            {
                var excInfo = new ExceptionInfoContext(_currentFrame.LastException);
                _operationStack.Push(excInfo);
            }
            else
            {
                _operationStack.Push(ExceptionInfoContext.EmptyExceptionInfo());
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
            if (_debugInfo != null)
            {
                _operationStack.Push(_debugInfo);
            }
            else
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
            }
            NextInstruction();
        }

        private void NewFunc(int argCount)
        {
            IValue[] argValues;

            if (argCount == 0)
                argValues = new IValue[0];
            else
            {
                var valueFromStack = _operationStack.Pop().GetRawValue();
                if (valueFromStack is IValueArray array)
                    argValues = array.ToArray();
                else
                    argValues = new IValue[0];
            }
            
            var typeName = _operationStack.Pop().AsString();
            if (!TypeManager.TryGetType(typeName, out var type))
            {
                throw RuntimeException.ConstructorNotFound(typeName);
            }
            
            // TODO убрать cast после рефакторинга ITypeFactory
            var factory = (TypeFactory)TypeManager.GetFactoryFor(type);
            var context = new TypeActivationContext
            {
                TypeName = typeName,
                TypeManager = _mem.TypeManager,
                Services = _mem.Services
            };

            var instance = factory.Activate(context, argValues);
            _operationStack.Push(instance);
            NextInstruction();
        }

        #endregion

        #endregion

        private StackRuntimeModule CompileExpressionModule(string expression)
        {
            var ctx = ExtractCompilerContext();
            var entryId = CurrentCodeEntry().ToString();

            var stringSource = SourceCodeBuilder.Create()
                .FromString(expression)
                .WithName($"{entryId}:<eval>")
                .Build();

            var compiler = _mem.Services.Resolve<EvalCompiler>();
            compiler.SharedSymbols = ctx;
            var module = (StackRuntimeModule)compiler.CompileExpression(stringSource);
            return module;
        }

        private StackRuntimeModule CompileExecutionBatchModule(string execBatch)
        {
            var ctx = ExtractCompilerContext();
            var entryId = CurrentCodeEntry().ToString();

            var stringSource = SourceCodeBuilder.Create()
                .FromString(execBatch)
                .WithName($"{entryId}:<exec>")
                .Build();
            
            var compiler = _mem.Services.Resolve<EvalCompiler>();
            compiler.SharedSymbols = ctx;
            var module = (StackRuntimeModule)compiler.CompileBatch(stringSource);
            
            return module;
        }

        private SymbolTable ExtractCompilerContext()
        {
            var ctx = new SymbolTable();
            foreach (var scope in _scopes)
            {
                var symbolScope = new SymbolScope();
                foreach (var methodInfo in scope.Methods)
                {
                    symbolScope.DefineMethod(methodInfo.ToSymbol());
                }
                foreach (var variable in scope.Variables)
                {
                    if (variable.SystemType.Alias != null)
                    {
                        symbolScope.DefineVariable(new AliasedVariableSymbol(variable.Name, variable.SystemType.Alias));
                    }
                    else
                    {
                        symbolScope.DefineVariable(new LocalVariableSymbol(variable.Name));
                    }
                }

                ctx.PushScope(symbolScope, scope.Instance);
            }

            var locals = new SymbolScope();
            foreach (var variable in _currentFrame.Locals)
            {
                locals.DefineVariable(new LocalVariableSymbol(variable.Name));
            }

            ctx.PushScope(locals, _scopes.Last().Instance);
            return ctx;
        }

        private void NextInstruction()
        {
            _currentFrame.InstructionPointer++;
        }

        private IValue PopRawValue()
        {
            return _operationStack.Pop().GetRawValue();
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

        private ExecutionFrameInfo FrameInfo(StackRuntimeModule module, ExecutionFrame frame)
        {
            return new ExecutionFrameInfo()
            {
                LineNumber = frame.LineNumber,
                MethodName = frame.MethodName,
                Source = module.Source.Location,
                FrameObject = frame
            };
        }

        // multithreaded instance
        [ThreadStatic]
        private static MachineInstance _currentThreadWorker;

        private static void SetCurrentMachineInstance(MachineInstance inst)
        {
            _currentThreadWorker = inst;
        }

        public static MachineInstance Current
        {
            get
            {
                if(_currentThreadWorker == null)
                    _currentThreadWorker = new MachineInstance();

                return _currentThreadWorker;
            }
        }
    }
}
