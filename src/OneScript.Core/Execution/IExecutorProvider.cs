/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Execution
{
    public delegate BslValue Invoker(
        IBslProcess process,
        BslObjectValue target,
        IExecutableModule module,
        BslScriptMethodInfo method,
        IValue[] arguments);

    public interface IExecutorProvider
    {
        Type SupportedModuleType { get; }

        Invoker GetInvokeDelegate();
        
        void BeforeProcessStart(IBslProcess process)
        {
        }

        void AfterProcessExit(IBslProcess process)
        {
        }

    }
}