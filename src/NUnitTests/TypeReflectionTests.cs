/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace NUnitTests
{
    [TestFixture]
    public class TypeReflectionTests
    {
        private ScriptingEngine host;

        [OneTimeSetUp]
        public void Init()
        {
            host = new ScriptingEngine();
            host.Environment = new RuntimeEnvironment();
        }
  
        private LoadedModule LoadFromString(string code)
        {
            var codeSrc = host.Loader.FromString(code);
            var cmp = host.GetCompilerService();
            var image = cmp.Compile(codeSrc);
            var module = host.LoadModuleImage(image);

            return module;
        }

        [Test]
        public void CheckIfTypeHasReflectedWithName()
        {
            string script = "Перем А;";

            var reflected = CreateDummyType(script);
            Assert.AreEqual("Dummy", reflected.Name);
            Assert.AreEqual("ScriptEngine.Machine.Contexts.dyn.Dummy", reflected.FullName);

        }

        private Type CreateDummyType(string script)
        {
            var module = LoadFromString(script);
            var builder = new ClassBuilder<UserScriptContextInstance>();
            var reflected = builder.SetModule(module)
                                   .SetTypeName("Dummy")
                                   .ExportDefaults()
                                   .Build();

            return reflected;
        }

        [Test]
        public void CheckNonExportVarsArePrivateFields()
        {
            string script = "Перем А; Перем Б Экспорт;";

            var reflected = CreateDummyType(script);

            var props = reflected.GetFields(BindingFlags.NonPublic);
            Assert.AreEqual(1, props.Length);
            Assert.AreEqual("А", props[0].Name);
            Assert.AreEqual(props[0].FieldType, typeof(IValue));
        }

        [Test]
        public void CheckExportVarsArePublicFields()
        {
            string script = "Перем А; Перем Б Экспорт;";

            var reflected = CreateDummyType(script);

            var props = reflected.GetFields(BindingFlags.Public);
            Assert.AreEqual(1, props.Length);
            Assert.AreEqual("Б", props[0].Name);
            Assert.AreEqual(props[0].FieldType, typeof(IValue));

        }

        [Test]
        public void CheckDefaultGetMethodsArePublic()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods();
            Assert.AreEqual(1, defaultGet.Length);
            Assert.AreEqual("Внешняя", defaultGet[0].Name);
        }

        [Test]
        public void CheckExplicitPublicMethodsCanBeRetrieved()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.Public);
            Assert.AreEqual(1, defaultGet.Length);
            Assert.AreEqual("Внешняя", defaultGet[0].Name);
        }

        [Test]
        public void CheckPrivateMethodsCanBeRetrieved()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.NonPublic);
            Assert.AreEqual(1, defaultGet.Length);
            Assert.AreEqual("Внутренняя", defaultGet[0].Name);
        }

        [Test]
        public void CheckAllMethodsCanBeRetrieved()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.Public|BindingFlags.NonPublic);
            Assert.AreEqual(2, defaultGet.Length);
        }

        [Test]
        public void ClassCanBeCreatedViaConstructor()
        {
            var cb = new ClassBuilder<UserScriptContextInstance>();
            var module = LoadFromString("");
            cb.SetTypeName("testDrive")            
                .SetModule(module)
                .ExportDefaults()
                .ExportConstructor((parameters => new UserScriptContextInstance(module)));
            var type = cb.Build();

            var instance = type.GetConstructors()[0].Invoke(new object[0]);
            Assert.IsInstanceOf<UserScriptContextInstance>(instance);
        }
    }
}
