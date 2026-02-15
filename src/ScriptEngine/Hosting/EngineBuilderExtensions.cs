/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Debugger;
using ScriptEngine.Machine.Interfaces;

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
        public static IEngineBuilder WithServices(this IEngineBuilder b, IServiceCollection ioc)
        {
            b.Services = ioc;
            return b;
        }
        
        public static IEngineBuilder SetupEnvironment(this IEngineBuilder b, Action<ExecutionContext> action)
        {
            b.EnvironmentProviders.Add(action);
            return b;
        }
        
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        public static IEngineBuilder SetDefaultOptions(this IEngineBuilder builder)
        {
            var services = builder.Services;
            
            services.AddSingleton<ITypeManager, DefaultTypeManager>();
            services.AddSingleton<IGlobalsManager, GlobalInstancesManager>();
            services.AddSingleton<RuntimeEnvironment>();
            services.AddSingleton<IRuntimeEnvironment>(sp => sp.GetRequiredService<RuntimeEnvironment>());
            services.AddSingleton<TypeSymbolsProviderFactory>();
            services.AddSingleton<IErrorSink>(svc => new ThrowingErrorSink(CompilerException.FromCodeError));
            services.AddSingleton<IExceptionInfoFactory, ExceptionInfoFactory>();
            services.AddSingleton<IBslProcessFactory, BslProcessFactory>();
            services.AddSingleton<IDebugger, DisabledDebugger>();
            services.AddSingleton<ContextDiscoverer>();

            services.AddScoped<StackMachineProvider>();
            
            services.AddTransient<IDependencyResolver, NullDependencyResolver>();
            
            services.TryAddEnumerable<IExecutorProvider, StackMachineExecutor>();
            services.TryAddEnumerable<IDirectiveHandler, ConditionalDirectiveHandler>();
            services.TryAddEnumerable<IDirectiveHandler, RegionDirectiveHandler>();
            
            services.AddTransient<ExecutionContext>();
            services.EnablePredefinedIterables();
            services.AddTransient<PreprocessorHandlers>(sp =>
            {
                var providers = sp.GetServices<IDirectiveHandler>();
                return new PreprocessorHandlers(providers);
            });
            
            services.AddSingleton<EngineConfiguration>();
            services.AddTransient<KeyValueConfig>(sp =>
            {
                var holder = sp.GetRequiredService<EngineConfiguration>();
                return holder.GetConfig();
            });
            
            services.AddTransient<OneScriptCoreOptions>(sp =>
            {
                var config = sp.GetRequiredService<KeyValueConfig>();
                return new OneScriptCoreOptions(config);
            });
            
            services.AddTransient<ScriptingEngine>();

            return builder;
        }

        public static IEngineBuilder UseImports(this IEngineBuilder b)
        {
            b.Services.UseImports();
            return b;
        }

        public static IEngineBuilder WithDebugger(this IEngineBuilder b, IDebugger debugger)
        {
            b.Services.AddSingleton(debugger);
            return b;
        }

        public static IEngineBuilder SetupServices(this IEngineBuilder b, Action<IServiceCollection> setup)
        {
            setup(b.Services);
            return b;
        }
    }
}
