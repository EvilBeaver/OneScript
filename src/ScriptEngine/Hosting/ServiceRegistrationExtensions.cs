/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
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
            b.Services.RegisterSingleton(b.ConfigurationProviders);
            return b;
        }
        
        public static MachineEnvironment AddAssembly(this MachineEnvironment env, Assembly asm, Predicate<Type> filter = null)
        {
            var discoverer = new ContextDiscoverer(env.TypeManager, env.GlobalInstances);
            discoverer.DiscoverClasses(asm, filter);
            discoverer.DiscoverGlobalContexts(env.GlobalNamespace, asm, filter);
            return env;
        }
        
        public static MachineEnvironment AddGlobalContext(this MachineEnvironment env, IAttachableContext context)
        {
            env.GlobalNamespace.InjectObject(context);
            env.GlobalInstances.RegisterInstance(context);
            return env;
        }

        public static IServiceDefinitions UseImports(this IServiceDefinitions services)
        {
            services.RegisterEnumerable<IDirectiveHandler, ImportDirectivesHandler>();
            services.RegisterSingleton<IDependencyResolver, NullDependencyResolver>();
            return services;
        }
        
        public static IServiceDefinitions AddDirectiveHandler<T>(this IServiceDefinitions services) where T : class, IDirectiveHandler
        {
            services.RegisterEnumerable<IDirectiveHandler, T>();
            return services;
        }
    }
}