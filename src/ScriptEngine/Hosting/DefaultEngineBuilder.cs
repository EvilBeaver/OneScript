/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine.Hosting
{
    public class DefaultEngineBuilder : IEngineBuilder
    {
        private DefaultEngineBuilder()
        {
        }
        
        public static IEngineBuilder Create()
        {
            return new DefaultEngineBuilder();
        }
        
        public RuntimeEnvironment Environment { get; set; } = new RuntimeEnvironment();
        public ITypeManager TypeManager { get; set; } = new DefaultTypeManager();
        public IGlobalsManager GlobalInstances { get; set; } = new GlobalInstancesManager();
        public ICompilerServiceFactory CompilerFactory { get; set; }
        public CompilerOptions CompilerOptions { get; set; }
        public IDebugController DebugController { get; set; }
        public ConfigurationProviders ConfigurationProviders { get; } = new ConfigurationProviders();
        public IServiceDefinitions Services { get; set; } = new TinyIocImplementation();

        public ScriptingEngine Build()
        {
            this.AddAssembly(GetType().Assembly);
            
            if(CompilerOptions == default)
                CompilerOptions = new CompilerOptions();
            
            if (CompilerFactory == default)
                CompilerFactory = new AstBasedCompilerFactory(CompilerOptions);

            var container = Services.CreateContainer();

            var engine = container.Resolve<ScriptingEngine>();
            
            // var engine = new ScriptingEngine(
            //     TypeManager,
            //     GlobalInstances,
            //     Environment,
            //     CompilerFactory,
            //     ConfigurationProviders);
            
            engine.DebugController = DebugController;

            var dependencyResolver = container.TryResolve<IDependencyResolver>();
            dependencyResolver?.Initialize(engine);
            
            return engine;
        }
    }
}