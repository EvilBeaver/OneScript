/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using FluentAssertions;
using OneScript.Language.SyntaxAnalysis;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
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

            var thisObjIndex = instance.FindProperty("ThisObject");
            var thisPropVal = instance.GetPropValue(thisObjIndex);
            thisPropVal.Should().BeSameAs((IValue)instance);
            
            var myVarPropertyIndex = instance.FindProperty("МояПеременная");
            var myVarValue = instance.GetPropValue(myVarPropertyIndex);
            myVarValue.Should().Be(UndefinedValue.Instance);

            thisObjIndex.Should().Be(0);
            instance.FindProperty("МояПеременная").Should().Be(1);
        }
        
        [Fact]
        public void ExternalVariables_And_Properties_Are_Accessible()
        {
            var context = new ExternalContextData();
            context.Add("ContextVariable", ValueFactory.Create(18));
            
            var code = "Перем МояПеременная Экспорт;";

            var instance = CompileUserScript(code, context);
            var externalVar = instance.FindProperty("ContextVariable");
            var value = instance.GetPropValue(externalVar);
            value.AsNumber().Should().Be(18);

            var allNames = instance.GetProperties()
                .Select(x => x.Identifier)
                .ToArray();

            allNames[0].Should().BeOneOf("ThisObject", "ЭтотОбъект");
            allNames[1].Should().Be("ContextVariable");
            allNames[2].Should().Be("МояПеременная");
        }

        private static IRuntimeContextInstance CompileUserScript(string code, ExternalContextData context = null)
        {
            var engine = DefaultEngineBuilder
                .Create()
                .Build();

            var compiler = engine.GetCompilerService();

            var source = engine.Loader.FromString(code);
            engine.Initialize();
            var image = engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, source, context);
            
            UserScriptContextInstance.PrepareCompilation(compiler);
            var instance = engine.NewObject(engine.LoadModuleImage(image), context);
            return instance;
        }
    }
}