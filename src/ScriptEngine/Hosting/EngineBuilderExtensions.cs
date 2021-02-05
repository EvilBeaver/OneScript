/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Hosting
{
    public static class EngineBuilderExtensions
    {
        public static IEngineBuilder WithServices(this IEngineBuilder b, IServiceDefinitions ioc)
        {
            b.Services = ioc;
            return b;
        }
        
        public static IEngineBuilder SetupEnvironment(this IEngineBuilder b, Action<ScriptingEngine, IServiceContainer> action)
        {
            b.StartupAction = action;
            return b;
        }
        
        public static IEngineBuilder AddAssembly(this IEngineBuilder b, Assembly asm, Predicate<Type> filter = null)
        {
            var discoverer = new ContextDiscoverer(b.TypeManager, b.GlobalInstances);
            discoverer.DiscoverClasses(asm, filter);
            discoverer.DiscoverGlobalContexts(b.Environment, asm, filter);
            return b;
        }
        
        public static IEngineBuilder AddGlobalContext(this IEngineBuilder b, IAttachableContext context)
        {
            b.Environment.InjectObject(context);
            b.GlobalInstances.RegisterInstance(context);
            return b;
        }
        
        public static IEngineBuilder SetDefaultOptions(this IEngineBuilder builder)
        {
            var services = builder.Services;
            
            services.Register<IServiceContainer>(sp => sp);
            services.RegisterSingleton<ITypeManager, DefaultTypeManager>();
            services.RegisterSingleton<IGlobalsManager, GlobalInstancesManager>();
            services.RegisterSingleton<RuntimeEnvironment>();
            services.RegisterSingleton<IAstBuilder, DefaultAstBuilder>();
            services.RegisterSingleton<ICompilerServiceFactory, AstBasedCompilerFactory>();
            services.RegisterSingleton<BslSyntaxWalker, AstBasedCodeGenerator>();
            services.RegisterSingleton<IErrorSink, ThrowingErrorSink>();
            services.RegisterSingleton<IDependencyResolver, NullDependencyResolver>();
            
            services.RegisterEnumerable<IDirectiveHandler, ConditionalDirectiveHandler>();
            services.RegisterEnumerable<IDirectiveHandler, RegionDirectiveHandler>();
            services.RegisterEnumerable<IDirectiveHandler, ImportDirectivesHandler>();
            
            services.Register<PreprocessorHandlers>(sp =>
            {
                var providers = sp.ResolveEnumerable<IDirectiveHandler>();
                return new PreprocessorHandlers(providers);
            });
            
            services.Register<CompilerOptions>(sp =>
            {
                var opts = new CompilerOptions
                {
                    DependencyResolver = sp.Resolve<IDependencyResolver>(),
                    ErrorSink = sp.Resolve<IErrorSink>(),
                    NodeBuilder = sp.Resolve<IAstBuilder>(),
                    PreprocessorHandlers = sp.Resolve<PreprocessorHandlers>()
                };
                
                return opts;
            });

            return builder;
        }

        public static IEngineBuilder WithDebugger(this IEngineBuilder b, IDebugController debugger)
        {
            b.DebugController = debugger;
            return b;
        }
    }
}