/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Native.Runtime;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Dynamic.Tests;

public class NativeExecutableTestProcess : IBslProcess
{
    private readonly IExecutorProvider _executorProvider = new NativeExecutorProvider();

    public BslValue Run(BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments)
    {
        return _executorProvider.GetInvokeDelegate()(this, target, module, method, arguments);
    }

    public IServiceContainer Services { get; set; }

    public int VirtualThreadId => 0;
}