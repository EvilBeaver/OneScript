/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Globalization;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Native.Compiler;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Native.Runtime
{
    public class NativeExecutorProvider : IExecutorProvider
    {
        public Type SupportedModuleType => typeof(DynamicModule);
        
        public Invoker GetInvokeDelegate()
        {
            return Executor;
        }

        private static BslValue Executor(
            BslObjectValue target,
            IExecutableModule module,
            BslMethodInfo method,
            IValue[] arguments)
        {
            if (!(module is DynamicModule))
            {
                throw new InvalidOperationException();
            }
            
            if (!(target is IAttachableContext context))
            {
                throw new InvalidOperationException();
            }
            
            if (!(method is BslNativeMethodInfo nativeMethod))
            {
                throw new InvalidOperationException();
            }

            return (BslValue) nativeMethod.Invoke(context, BindingFlags.Default, null, arguments, CultureInfo.CurrentCulture);

        }
    }
}
