/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    internal class BslProcess : IBslProcess
    {
        private readonly IExecutorProvider[] _executorProviders;
        private readonly IDictionary<Type, Invoker> _bslExecutorsByModule;

        private bool _isRunning;
        private bool _disposed;
        
        public BslProcess(int id, ExecutionContext context, IEnumerable<IExecutorProvider> executorProviders)
        {
            _executorProviders = executorProviders.ToArray();
            _bslExecutorsByModule =
                _executorProviders.ToDictionary(item => item.SupportedModuleType, item => item.GetInvokeDelegate());
            
            VirtualThreadId = id;
            Services = context.Services.CreateScope();
        }

        public IServiceContainer Services { get; }

        public int VirtualThreadId { get; }

        public IBslExecutionThread ExecutionThread { get; set; }

        /// <summary>
        /// Завершает процесс: единица исполнения отработала.
        ///
        /// Поток снимается с процесса после завершения, а не до: обработчик ПриЗавершении вправе
        /// обратиться к ТекущийПоток() и должен получить свой поток, а не новый и пустой.
        ///
        /// Область сервисов освобождается последней: завершение потока обращается к ней за
        /// процессором событий.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            ExecutionThread?.Terminate();
            ExecutionThread = null;

            Services.Dispose();
        }

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
                    Array.ForEach(_executorProviders, e => e.AfterProcessExit(this));
                    _isRunning = false;
                }
            }
        }
    }
}