/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public class StackMachineExecutor : IExecutorProvider
    {
        private MachineInstance _machine;

        public Type SupportedModuleType => typeof(StackRuntimeModule);
        
        public Invoker GetInvokeDelegate()
        {
            return Executor;
        }

        public void BeforeProcessStart(IBslProcess process)
        {
            _machine = new MachineInstance();
            _machine.Setup(process);
            
            var debugger = process.Services.TryResolve<IDebugController>();
            if (debugger != default)
            {
                _machine.SetDebugMode(debugger.ThreadManager, debugger.BreakpointManager);
                debugger.ThreadManager.ThreadStarted(process.VirtualThreadId, _machine);
            }

            process.Services.Resolve<StackMachineProvider>().Machine = _machine;
        }

        public void AfterProcessExit(IBslProcess process)
        {
            var debugger = process.Services.TryResolve<IDebugController>();
            debugger?.ThreadManager.ThreadExited(process.VirtualThreadId);
        }

        private BslValue Executor(IBslProcess process, BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments)
        {
            if (!(method is MachineMethodInfo scriptMethodInfo))
            {
                throw new InvalidOperationException($"Method has type {method?.GetType()} but expected {typeof(MachineMethodInfo)}");
            }
            
            if (!(target is IRunnable runnable))
            {
                throw new InvalidOperationException($"Target must implement {typeof(IRunnable)}");
            }

            if (_machine?.Process == default)
            {
                throw new InvalidOperationException("Machine is not initialized by process");
            }
            
            return (BslValue)_machine.ExecuteMethod(runnable, scriptMethodInfo, arguments);
        }
    }
}