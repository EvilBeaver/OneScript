/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Execution
{
    /// <summary>
    /// Класс процесса который не может выполняться.
    /// Используется в API, требующих процесс, когда реальное исполнение bsl-кода не предполагается.
    /// При попытке запустить в таком процессе BSL-код будет выдано исключение.
    /// </summary>
    public class ForbiddenBslProcess : IBslProcess
    {
        public static readonly IBslProcess Instance = new ForbiddenBslProcess();
            
        private ForbiddenBslProcess()
        {}
                
        public BslValue Run(BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments) 
            => throw new NotSupportedException("BslProcess required");

        public IServiceContainer Services => throw new NotSupportedException("BslProcess required");

        public int VirtualThreadId => -1;
    }
}