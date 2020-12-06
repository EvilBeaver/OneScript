/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;

namespace ScriptEngine.Hosting
{
    public class DefaultEngineBuilder : IEngineBuilder
    {
        public static IEngineBuilder Create()
        {
            return new DefaultEngineBuilder();
        }
        
        public RuntimeEnvironment Environment { get; set; }
        public ITypeManager TypeManager { get; set; }
        public IGlobalsManager GlobalInstances { get; set; }
        public ICompilerServiceFactory CompilerFactory { get; set; }
        
        public ScriptingEngine Build()
        {
            if(Environment == default)
                Environment = new RuntimeEnvironment();
            
            if(CompilerFactory == default)
                CompilerFactory = new AstBasedCompilerFactory();
            
            if(GlobalInstances == default)
                GlobalInstances = new GlobalInstancesManager();
            
            if(TypeManager == default)
                TypeManager = new StandartTypeManager();
            
            var engine = new ScriptingEngine(TypeManager, GlobalInstances, Environment, CompilerFactory);
            return engine;
        }
    }
}