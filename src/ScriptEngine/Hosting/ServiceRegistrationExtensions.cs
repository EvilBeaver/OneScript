/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Hosting
{
    public static class ServiceRegistrationExtensions
    {
        public static IEngineBuilder SetupConfiguration(this IEngineBuilder b, Action<ConfigurationProviders> setup)
        {
            setup(b.ConfigurationProviders);
            b.Services.AddSingleton(b.ConfigurationProviders);
            return b;
        }
        
        public static ExecutionContext AddAssembly(this ExecutionContext env, Assembly asm, Predicate<Type> filter = null)
        {
            var discoverer = env.Services.Resolve<ContextDiscoverer>();
            discoverer.DiscoverClasses(asm, filter);
            discoverer.DiscoverGlobalContexts(env.GlobalNamespace, asm, filter);
            return env;
        }
        
        public static ExecutionContext AddGlobalContext(this ExecutionContext env, IAttachableContext context)
        {
            env.GlobalNamespace.InjectObject(context);
            env.GlobalInstances.RegisterInstance(context);
            return env;
        }

        public static IServiceCollection UseImports(this IServiceCollection services)
        {
            services.TryAddEnumerable<IDirectiveHandler, ImportDirectivesHandler>();
            services.AddSingleton<IDependencyResolver, NullDependencyResolver>();
            return services;
        }
        
        public static IServiceCollection UseImports<T>(this IServiceCollection services)
            where T : class, IDependencyResolver
        {
            services.TryAddEnumerable<IDirectiveHandler, ImportDirectivesHandler>();
            services.AddSingleton<IDependencyResolver, T>();
            return services;
        }
        
        public static IServiceCollection UseImports(this IServiceCollection services, Func<IServiceProvider, IDependencyResolver> factory)
        {
            services.TryAddEnumerable<IDirectiveHandler, ImportDirectivesHandler>();
            services.AddSingleton<IDependencyResolver>(factory);
            return services;
        }
        
        public static IServiceCollection AddDirectiveHandler<T>(this IServiceCollection services) where T : class, IDirectiveHandler
        {
            services.TryAddEnumerable<IDirectiveHandler, T>();
            return services;
        }

        public static void TryAddEnumerable<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));
        }
    }
}