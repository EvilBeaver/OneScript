/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.BinaryData;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary
{
    public static class EngineBuilderExtensions
    {
        /// <summary>
        /// Регистрирует лимит памяти для «ДвоичныеДанные» из конфигурации движка (ключ <see cref="BinaryDataMemoryLimitConfiguration.InMemoryMaxBytesConfigKey"/>).
        /// Вызывайте после <c>SetDefaultOptions</c> из <c>ScriptEngine.Hosting.EngineBuilderExtensions</c>.
        /// </summary>
        public static IEngineBuilder RegisterBinaryDataMemoryLimitFromConfig(this IEngineBuilder builder)
        {
            builder.Services.RegisterSingleton<IBinaryDataMemoryLimit>(sp =>
            {
                var cfg = sp.Resolve<KeyValueConfig>();
                var raw = cfg[BinaryDataMemoryLimitConfiguration.InMemoryMaxBytesConfigKey];
                var bytes = BinaryDataMemoryLimitConfiguration.ResolveFromConfigString(raw, SystemLogger.Write);
                return new BinaryDataMemoryLimit(bytes);
            });

            return builder;
        }

        public static ExecutionContext AddStandardLibrary(this ExecutionContext env)
        {
            return env.AddAssembly(typeof(ArrayImpl).Assembly);
        }
    }
}
