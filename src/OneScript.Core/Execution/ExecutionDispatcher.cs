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
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Execution
{
    public class ExecutionDispatcher
    {
        private readonly Dictionary<Type, Invoker> _invokers;
        
        public ExecutionDispatcher(IEnumerable<IExecutorProvider> providers)
        {
            _invokers = providers.ToDictionary(item => item.SupportedModuleType, item => item.GetInvokeDelegate());
        }

        private static ExecutionDispatcher _dispatcher;

        public static ExecutionDispatcher Current
        {
            get => _dispatcher;
            set
            {
                // если нужно будет для тестов менять диспетчер - можно будет разрешить запись
                if (_dispatcher != null)
                    throw new InvalidOperationException("Can't change dispatcher after setup");

                _dispatcher = value;
            }
        }

        public IValue Execute(BslObjectValue target, IExecutableModule module, BslMethodInfo method, IValue[] arguments)
        {
            return _invokers[module.GetType()].Invoke(target, module, method, arguments);
        }
    }
}