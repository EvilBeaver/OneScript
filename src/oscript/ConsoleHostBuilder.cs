/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.HostedScript.Extensions;
using OneScript.Web.Server;

namespace oscript
{
    internal static class ConsoleHostBuilder
    {
        public static IEngineBuilder Create(string codePath)
        {
            var builder = DefaultEngineBuilder.Create()
                .SetupConfiguration(p =>
                {
                    p.UseSystemConfigFile()
                        .UseEntrypointConfigFile(codePath)
                        .UseEnvironmentVariableConfig("OSCRIPT_CONFIG");
                });

            BuildUpWithIoC(builder);
            
            builder.SetupEnvironment(env =>
                {
                    env.AddStandardLibrary()
                     .AddWebServer()
                     .UseTemplateFactory(new DefaultTemplatesFactory(env.Services.Resolve<IBinaryDataMemoryLimit>()));
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
                .UseDefaultHosting()
                .UseNativeRuntime()
                .UseEventHandlers();
        }
    }
}