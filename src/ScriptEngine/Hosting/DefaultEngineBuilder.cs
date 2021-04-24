/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DependencyInjection;
using ScriptEngine.Machine;

namespace ScriptEngine.Hosting
{
    public class DefaultEngineBuilder : IEngineBuilder
    {
        protected DefaultEngineBuilder()
        {
            EnvironmentProviders.Add(environment => environment.AddAssembly(GetType().Assembly));
        }
        
        public static IEngineBuilder Create()
        {
            var builder = new DefaultEngineBuilder();
            return builder;
        }
        
        public IDebugController DebugController { get; set; }
        public ConfigurationProviders ConfigurationProviders { get; } = new ConfigurationProviders();

        public EnvironmentProviders EnvironmentProviders { get; } = new EnvironmentProviders();
        
        public IServiceDefinitions Services { get; set; } = new TinyIocImplementation();
        
        public virtual ScriptingEngine Build()
        {
            var container = GetContainer();

            var engine = container.Resolve<ScriptingEngine>();
            var env = container.Resolve<MachineEnvironment>();
            
            EnvironmentProviders.Invoke(env);
            
            engine.DebugController = DebugController;

            var dependencyResolver = container.TryResolve<IDependencyResolver>();
            dependencyResolver?.Initialize(engine);
            
            return engine;
        }

        protected virtual IServiceContainer GetContainer()
        {
            return Services.CreateContainer();
        }
    }
}