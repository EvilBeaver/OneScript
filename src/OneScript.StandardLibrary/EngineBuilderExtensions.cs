/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.NativeApi;
using OneScript.Types;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary
{
    public static class EngineBuilderExtensions
    {
        public static IEngineBuilder UseBinaryDataOptions(this IEngineBuilder builder)
        {
            builder.Services.RegisterSingleton<IBinaryDataMemoryLimit, BinaryDataOptions>();
            return builder;
        }

        public static IEngineBuilder UseNativeApi(this IEngineBuilder builder)
        {
            builder.Services.RegisterEnumerable<ILazyTypeResolver, NativeApiLazyTypeResolver>();
            return builder;
        }
        
        public static ExecutionContext AddStandardLibrary(this ExecutionContext env)
        {
            return env.AddAssembly(typeof(ArrayImpl).Assembly);
        }
    }
}
