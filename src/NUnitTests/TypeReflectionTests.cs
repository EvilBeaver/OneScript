using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

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
  
        private LoadedModuleHandle LoadFromString(string code)
        {
            var codeSrc = host.Loader.FromString(code);
            var cmp = host.GetCompilerService();
            var image = cmp.CreateModule(codeSrc);
            var module = host.LoadModuleImage(image);

            return module;
        }

        [Test]
        public void CheckIfTypeHasReflectedWithName()
        {
            string script = "Перем А;";

            var module = LoadFromString(script);
            var reflected = ReflectedClassType.ReflectModule(module, "Dummy");

            Assert.AreEqual("Dummy", reflected.Name);
            Assert.AreEqual("ScriptEngine.Machine.Contexts.dyn.Dummy", reflected.FullName);

        }

        [Test]
        [Ignore("Еще не сделано")]
        public void CheckNonExportVarsAreFields()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CheckExportVarsAreProperties()
        {
            string script = "Перем А; Перем Б Экспорт;";

            var module = LoadFromString(script);
            var reflected = ReflectedClassType.ReflectModule(module, "Dummy");

            var props = reflected.GetProperties();
            Assert.AreEqual(1, props.Length);
            Assert.AreEqual("Б", props[0].Name);
            Assert.AreEqual(props[0].PropertyType, typeof(IValue));

        }
    }
}
