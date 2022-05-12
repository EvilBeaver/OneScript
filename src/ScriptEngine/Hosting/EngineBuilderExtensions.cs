/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Types;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;

namespace ScriptEngine.Hosting
{
    public static class EngineBuilderExtensions
    {
        /// <summary>
        /// Используется для замены DI системы, например в ASP.NET
        /// </summary>
        /// <param name="b"></param>
        /// <param name="ioc"></param>
        /// <returns></returns>
        public static IEngineBuilder WithServices(this IEngineBuilder b, IServiceDefinitions ioc)
        {
            b.Services = ioc;
            return b;
        }
        
        public static IEngineBuilder SetupEnvironment(this IEngineBuilder b, Action<ExecutionContext> action)
        {
            b.EnvironmentProviders.Add(action);
            return b;
        }
        
        public static IEngineBuilder SetDefaultOptions(this IEngineBuilder builder)
        {
            var services = builder.Services;
            
            services.Register<IServiceContainer>(sp => sp);
            services.RegisterSingleton<ITypeManager, DefaultTypeManager>();
            services.RegisterSingleton<IGlobalsManager, GlobalInstancesManager>();
            services.RegisterSingleton<RuntimeEnvironment>();
            services.RegisterSingleton<ICompilerServiceFactory, CompilerServiceFactory>();
            services.RegisterSingleton<IErrorSink>(svc => new ThrowingErrorSink(CompilerException.FromCodeError));
            
            services.Register<ExecutionDispatcher>();
            services.Register<IDependencyResolver, NullDependencyResolver>();
            
            services.RegisterEnumerable<IExecutorProvider, StackMachineExecutor>();
            services.RegisterEnumerable<IDirectiveHandler, ConditionalDirectiveHandler>();
            services.RegisterEnumerable<IDirectiveHandler, RegionDirectiveHandler>();
            
            services.Register<ExecutionContext>();
            
            services.Register<PreprocessorHandlers>(sp =>
            {
                var providers = sp.ResolveEnumerable<IDirectiveHandler>();
                return new PreprocessorHandlers(providers);
            });
            
            services.Register<KeyValueConfig>(sp =>
            {
                var providers = sp.Resolve<ConfigurationProviders>();
                return providers.CreateConfig();
            });
            
            services.Register<ScriptingEngine>();

            return builder;
        }

        public static IEngineBuilder UseImports(this IEngineBuilder b)
        {
            b.Services.UseImports();
            return b;
        }

        public static IEngineBuilder WithDebugger(this IEngineBuilder b, IDebugController debugger)
        {
            b.Services.RegisterSingleton(debugger);
            return b;
        }
    }
}