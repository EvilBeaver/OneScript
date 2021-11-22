/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Execution
{
    public static class ExecutionExtensions
    {
        public static void ExecuteModuleBody(this ExecutionDispatcher dispatcher, BslObjectValue target, IExecutableModule module)
        {
            var bodyMethod = module.ModuleBody;
            if (bodyMethod != default)
            {
                dispatcher.Execute(target, module, bodyMethod, Array.Empty<IValue>());
            }
        }
    }
}