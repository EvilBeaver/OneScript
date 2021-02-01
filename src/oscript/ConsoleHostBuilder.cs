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
            
            builder
                .AddAssembly(typeof(ArrayImpl).Assembly)
                .UseFileSystemLibraries();
            
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
            
            services.RegisterSingleton<ITypeManager, DefaultTypeManager>();
            services.RegisterSingleton<IGlobalsManager, GlobalInstancesManager>();
            services.RegisterSingleton<RuntimeEnvironment>();
            services.RegisterSingleton<IDependencyResolver, FileSystemDependencyResolver>();
            
            services.Register<IAstBuilder, DefaultAstBuilder>();
            services.Register<ICompilerServiceFactory, AstBasedCompilerFactory>();
            services.Register<BslSyntaxWalker, AstBasedCodeGenerator>();
            services.Register<IErrorSink, ThrowingErrorSink>();
            
            services.RegisterEnumerable<IDirectiveHandler, ConditionalDirectiveHandler>();
            services.RegisterEnumerable<IDirectiveHandler, RegionDirectiveHandler>();
            services.RegisterEnumerable<IDirectiveHandler, ImportDirectivesHandler>();
            
            services.Register<PreprocessorHandlers>(sp =>
            {
                var providers = sp.ResolverEnumerable<IDirectiveHandler>();
                return new PreprocessorHandlers(providers);
            });
            
            services.Register<CompilerOptions>(sp =>
            {
                var opts = new CompilerOptions
                {
                    DependencyResolver = sp.Resolve<IDependencyResolver>(),
                    ErrorSink = sp.Resolve<IErrorSink>(),
                    NodeBuilder = sp.Resolve<IAstBuilder>()
                };
                
                return opts;
            });
        }
    }
}