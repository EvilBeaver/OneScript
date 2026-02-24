/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.StandardLibrary;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ValueConvertersTest
    {
        [Fact]
        public void CallsConverterForParameter()
        {
            var engine = CreateEngine();
            var compiler = engine.GetCompilerService();
            
            var code = engine.Loader.FromString(
                "Перем Результат Экспорт;\n" +
                "Результат = Instance.КонвертацияПараметра(Новый Структура(\"Integer\", 8));");

            var context = new ExternalContextData();
            context.Add("Instance", new TestClassWithConverters());
            
            var module = engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, code, context, engine.NewProcess());
            
            var instance = engine.NewObject(module, engine.NewProcess(), context);
            var result = instance.GetPropValue("Результат");
            result.SystemType.Should().Be(BasicTypes.Number);
            result.AsNumber().Should().Be(8);
        }
        
        [Fact]
        public void CallsConverterForConstructorParameter()
        {
            var engine = CreateEngine();
            var compiler = engine.GetCompilerService();

            var code = engine.Loader.FromString(
                "Перем Результат Экспорт;\n" +
                "Результат = Новый КлассСКонвертером(Новый Структура(\"Integer\", 8));");
            
            var module = engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, code, null, engine.NewProcess());
            
            var instance = engine.NewObject(module, engine.NewProcess());
            var result = instance.GetPropValue("Результат") as TestClassWithConverters;
            result.Should().NotBeNull();
            result!.ValueFromConstructor.Integer.Should().Be(8);
        }

        private static ScriptingEngine CreateEngine()
        {
            var engine = DefaultEngineBuilder
                .Create()
                .SetDefaultOptions()
                .SetupEnvironment(builder => 
                    builder
                        .AddAssembly(typeof(ValueConvertersTest).Assembly)
                        .AddStandardLibrary()
                )
                .Build();
            
            engine.Initialize();
            return engine;
        }
    }
}