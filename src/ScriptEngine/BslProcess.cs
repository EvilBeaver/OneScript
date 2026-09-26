/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;

namespace ScriptEngine
{
    internal class BslProcess : IBslProcess
    {
        private static readonly string[] TerminationEventNames = { "ПриЗавершении", "OnTermination" };
        
        private readonly IExecutorProvider[] _executorProviders;
        private readonly IDictionary<Type, Invoker> _bslExecutorsByModule;

        private bool _isRunning;
        private bool _disposed;
        private CancellationToken _cancellationToken;
        private ProcessResourceLocks _resourceLocks;
        
        public BslProcess(int id, ExecutionContext context, IEnumerable<IExecutorProvider> executorProviders,
            CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _executorProviders = executorProviders.ToArray();
            _bslExecutorsByModule =
                _executorProviders.ToDictionary(item => item.SupportedModuleType, item => item.GetInvokeDelegate());
            
            VirtualThreadId = id;
            Services = context.Services.CreateScope();
        }

        public IServiceContainer Services { get; }

        public int VirtualThreadId { get; }

        public CancellationToken CancellationToken => _cancellationToken;

        internal ProcessResourceLocks ResourceLocks => _resourceLocks ??= new ProcessResourceLocks();

        public BslValue Run(BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments)
        {
            var notifyExecutors = !_isRunning;
            if (notifyExecutors)
            {
                Array.ForEach(_executorProviders, e => e.BeforeProcessStart(this));
            }

            _isRunning = true;

            try
            {
                return _bslExecutorsByModule[module.GetType()](this, target, module, method, arguments);
            }
            finally
            {
                if (notifyExecutors)
                {
                    // Обработчики завершения должны отработать и у отмененного процесса
                    _cancellationToken = CancellationToken.None;
                    RaiseTerminationEvent();
                    if (BslWrapper is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    
                    // Монитор привязан к потоку: не отпущенные процессом блокировки больше никто не освободит
                    _resourceLocks?.ReleaseAll();
                    Array.ForEach(_executorProviders, e => e.AfterProcessExit(this));
                    Services.Dispose();
                    _isRunning = false;
                }
            }
        }

        public IRuntimeContextInstance BslWrapper { get; set; }

        private void RaiseTerminationEvent()
        {
            if (BslWrapper == null)
                return;
            
            var eventProcessor = Services.TryResolve<IEventProcessor>();
            if (eventProcessor == null)
                return;

            try
            {
                foreach (var eventName in TerminationEventNames)
                {
                    try
                    {
                        eventProcessor.HandleEvent(BslWrapper, eventName, Array.Empty<IValue>(), this);
                    }
                    catch (Exception exception)
                    {
                        SystemLogger.Write(
                            $"WARNING! Error in execution thread termination handler '{eventName}': {exception.Message}");
                    }
                }
            }
            finally
            {
                // Процессор событий держит источник, пока подписки не сняты. Потоков исполнения
                // много и живут они недолго, поэтому без явного снятия реестр рос бы бесконечно.
                eventProcessor.RemoveAllHandlers(BslWrapper);
            }
        }
    }
}