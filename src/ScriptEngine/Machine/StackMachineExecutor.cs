/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Sources;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public class StackMachineExecutor : IExecutorProvider
    {
        private readonly ExecutionContext _environment;

        public StackMachineExecutor(ExecutionContext environment)
        {
            _environment = environment;
        }
        
        public Type SupportedModuleType => typeof(StackRuntimeModule);
        
        public Invoker GetInvokeDelegate()
        {
            return Executor;
        }

        private BslValue Executor(BslObjectValue target, IExecutableModule module, BslMethodInfo method, IValue[] arguments)
        {
            if (!(method is MachineMethodInfo scriptMethodInfo))
            {
                throw new InvalidOperationException();
            }
            
            if (!(target is IRunnable runnable))
            {
                throw new InvalidOperationException();
            }
            
            var currentMachine = MachineInstance.Current;
            if (!currentMachine.IsRunning)
            {
                currentMachine.SetMemory(_environment);
            }
            
            return (BslValue)currentMachine.ExecuteMethod(runnable, scriptMethodInfo, arguments);
        }
    }
}