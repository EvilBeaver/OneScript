/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class SymbolsRegistrationTests
    {
        [Fact]
        public void GlobalProperty_In_Shared_PropertyBag_IsSettable()
        {
            var env = new RuntimeEnvironment();
            var prop = new TestContextClass();
            
            env.InjectGlobalProperty(null, "TestClass", false);
            env.SetGlobalProperty("TestClass", prop);

            var storedProp = env.GetGlobalProperty("TestClass");
            Assert.Same(prop, storedProp);
        }
        
        [Fact]
        public void GlobalProperty_In_Separate_InjectedContext_IsSettable()
        {
            var engine = DefaultEngineBuilder
                .Create()
                .SetDefaultOptions()
                .Build();

            var compiler = engine.GetCompilerService();
            UserScriptContextInstance.PrepareCompilation(compiler);
            var module = compiler.Compile(engine.Loader.FromString("Перем А Экспорт; Перем Б Экспорт;\n" +
                                                                  "А = 1; Б = 2;"));
            engine.Initialize();
            var propertyHolder = (UserScriptContextInstance)engine.NewObject(module);
            engine.Dispose();
            
            var env = new RuntimeEnvironment();
            var propToPropBag = new TestContextClass();
            
            env.InjectObject(propertyHolder);
            env.InjectGlobalProperty(propToPropBag, "TestClass", false);
            
            Assert.Same(propToPropBag, env.GetGlobalProperty("TestClass"));
            Assert.Equal(ValueFactory.Create(1), env.GetGlobalProperty("А"));
            Assert.Equal(ValueFactory.Create(2), env.GetGlobalProperty("Б"));
            
        }
    }
}