using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class RuntimeFacadeTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Import_Of_Conflicting_Contexts_Is_Not_Allowed()
        {
            var engine = new OneScriptRuntime();

            var ctx1 = new ImportedMembersClass();
            var ctx2 = new ImportedMembersClass();

            engine.InjectObject(ctx1);
            engine.InjectObject(ctx2);
        }

        [TestMethod]
        public void Variable_Value_Is_Changed()
        {
            var rt = new OneScriptRuntime();
            var externalValue = new InjectedVariable("А", ValueFactory.Create(0));
            rt.InjectVariable(externalValue);
            var code = new StringCodeSource("А = 1;");
            var module = rt.Compile(code);
            rt.Execute(module, module.EntryPointName);

            Assert.AreEqual(BasicTypes.Number, externalValue.Type);
            Assert.AreEqual(1m, externalValue.AsNumber());
        }
    }
}
