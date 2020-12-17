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
        public static IEngineBuilder Create()
        {
            return new DefaultEngineBuilder();
        }
        
        public RuntimeEnvironment Environment { get; set; } = new RuntimeEnvironment();
        public ITypeManager TypeManager { get; set; } = new StandartTypeManager();
        public IGlobalsManager GlobalInstances { get; set; } = new GlobalInstancesManager();
        public ICompilerServiceFactory CompilerFactory { get; set; }
        public CompilerBuildOptions CompilerOptions { get; set; }
        public IDebugController DebugController { get; set; }
        public ConfigurationProviders ConfigurationProviders { get; } = new ConfigurationProviders();

        public ScriptingEngine Build()
        {
            if(CompilerFactory == default)
                CompilerFactory = new AstBasedCompilerFactory(CompilerOptions);
            
            var engine = new ScriptingEngine(TypeManager, GlobalInstances, Environment, CompilerFactory);
            
            return engine;
        }
    }
}