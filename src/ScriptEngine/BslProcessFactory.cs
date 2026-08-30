/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading;
using OneScript.DependencyInjection;
using OneScript.Execution;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;

namespace ScriptEngine
{
    /// <summary>
    /// Типовая фабрика bsl-процессов на базе диспетчера исполнителей
    /// </summary>
    public class BslProcessFactory : IBslProcessFactory
    {
        private readonly IServiceContainer _services;
        private int _threadIdCounter = 0;

        public BslProcessFactory(IServiceContainer services)
        {
            _services = services;
        }

        public IBslProcess NewProcess()
        {
            // Создаем новый контекст со всеми зависимостями
            var context = _services.Resolve<ExecutionContext>();
            var executors = _services.ResolveEnumerable<IExecutorProvider>();
                
            return new BslProcess(Interlocked.Increment(ref _threadIdCounter), context, executors);
        }
    }
}