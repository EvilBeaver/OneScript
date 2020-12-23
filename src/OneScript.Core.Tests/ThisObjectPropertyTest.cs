/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ThisObjectPropertyTest
    {
        [Fact]
        public void ThisObjectCanBeFoundForUserScript()
        {
            var builder = new DefaultEngineBuilder();
            var engine = builder.Build();
            engine.Initialize();
            var instance = engine.AttachedScriptsFactory.LoadFromString(engine.GetCompilerService(), "");
            
            instance.Should().BeOfType<UserScriptContextInstance>();
            instance.FindProperty("ЭтотОбъект").Should().NotBe(-1);
            instance.FindProperty("ThisObject").Should().NotBe(-1);

            var id = instance.FindProperty("ThisObject");
            instance.GetPropValue(id).Should().BeSameAs((IValue)instance);
            instance.IsPropReadable(id).Should().BeTrue();
            instance.IsPropWritable(id).Should().BeFalse();
            instance.GetPropName(id).Should().BeOneOf("ЭтотОбъект", "ThisObject");
        }
    }
}