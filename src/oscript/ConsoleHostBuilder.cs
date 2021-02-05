/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;
using OneScript.Language.SyntaxAnalysis;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Machine;

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
                        .UseEnvironmentVariableConfig("OSCRIPT_CONFIG")
                        .UseEntrypointConfigFile(codePath);
                });

            BuildUpWithIoC(builder);
            var x = new Lazy<string>(() => "hi");
            builder.SetupEnvironment((engine, container) =>
                {
                    engine.AttachAssembly(typeof(ArrayImpl).Assembly);
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
            var services = builder.Services;

            var config = builder.ConfigurationProviders.CreateConfig();
            services.RegisterSingleton(config);

            builder.SetDefaultOptions()
                .UseFileSystemLibraries();
        }
    }
}