/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.HostedScript.Extensions;

namespace oscript
{
    internal static class ConsoleHostBuilder
    {
        /// <summary>
        /// Создать построитель движка с файлом конфигурации возле точки входа.
        /// </summary>
        /// <param name="codePath">Путь точки входа. Нужен для определения расположения конфиг-файла oscript.cfg</param>
        /// <returns>Преднастроеный построитель движка для консольного приложения</returns>
        public static IEngineBuilder Create(string codePath)
        {
            var builder = DefaultEngineBuilder.Create()
                .SetupConfiguration(p =>
                {
                    p.UseSystemConfigFile()
                        .UseEnvironmentVariableConfig("OSCRIPT_CONFIG")
                        .UseEntrypointConfigFile(codePath);
                });

            BuildUpWithIoC(builder);
            
            builder.SetupEnvironment(e =>
                {
                    e.AddStandardLibrary()
                     .UseTemplateFactory(new DefaultTemplatesFactory());
                });

            return builder;
        }

        public static HostedScriptEngine Build(IEngineBuilder builder)
        {
            var engine = builder.Build(); 
            var mainEngine = new HostedScriptEngine(engine);

            return mainEngine;
        }

        private static void BuildUpWithIoC(IEngineBuilder builder)
        {
            builder.SetDefaultOptions()
                .UseImports()
                .UseFileSystemLibraries()
                .UseNativeRuntime()
                ;
        }
    }
}