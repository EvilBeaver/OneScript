/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using FluentAssertions;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TestUserScriptProperties
    {
        [Fact]
        public void ExportVariables_And_Properties_Are_Accessible()
        {
            var code = "Перем МояПеременная Экспорт;";

            var instance = CompileUserScript(code);

            var thisObjIndex = instance.GetPropertyNumber("ThisObject");
            var thisPropVal = instance.GetPropValue(thisObjIndex);
            thisPropVal.Should().BeSameAs((IValue)instance);
            
            var myVarPropertyIndex = instance.GetPropertyNumber("МояПеременная");
            var myVarValue = instance.GetPropValue(myVarPropertyIndex);
            myVarValue.Should().Be(BslUndefinedValue.Instance);

            thisObjIndex.Should().Be(0);
            instance.GetPropertyNumber("МояПеременная").Should().Be(1);
        }
        
        [Fact]
        public void ExternalVariables_And_Properties_Are_Accessible()
        {
            var context = new ExternalContextData();
            context.Add("ContextVariable", ValueFactory.Create(18));
            
            var code = "Перем МояПеременная Экспорт;";

            var instance = CompileUserScript(code, context);
            var externalVar = instance.GetPropertyNumber("ContextVariable");
            var value = instance.GetPropValue(externalVar);
            value.AsNumber().Should().Be(18);

            var allNames = instance.GetProperties()
                .Select(x => x.Name)
                .ToArray();

            allNames[0].Should().BeOneOf("ThisObject", "ЭтотОбъект");
            allNames[1].Should().Be("ContextVariable");
            allNames[2].Should().Be("МояПеременная");
        }

        private static IRuntimeContextInstance CompileUserScript(string code, ExternalContextData context = null)
        {
            var engine = DefaultEngineBuilder
                .Create()
                .SetDefaultOptions()
                .Build();

            var compiler = engine.GetCompilerService();

            var source = engine.Loader.FromString(code);
            engine.Initialize();
            var module = engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, source, context);
            
            UserScriptContextInstance.PrepareCompilation(compiler);
            var instance = engine.NewObject(module, context);
            return instance;
        }
    }
}