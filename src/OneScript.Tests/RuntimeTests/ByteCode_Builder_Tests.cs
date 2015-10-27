using System;
using OneScript.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OneScript.Tests.RuntimeTests
{
    [TestClass]
    public class ByteCode_Builder_Tests
    {
        [TestMethod]
        public void BCB_Returns_A_Compiled_Module()
        {
            var module = CreateModuleForCode("а = 1;");
            Assert.IsNotNull(module);
            Assert.IsInstanceOfType(module, typeof(CompiledModule));
        }

        private static CompiledModule CreateModuleForCode(string codeString)
        {
            var rt = new OneScriptRuntime();
            var code = new StringCodeSource(codeString);
            var module = rt.Compile(code) as CompiledModule;
            return module;
        }

        [TestMethod]
        public void SimpleAssignmentByteCode()
        {
            var module = CreateModuleForCode("а = 1;");
            var code = module.Commands;

            Assert.AreEqual(3, code.Count);
            Assert.AreEqual(OperationCode.PushVar, code[0].Code);
            Assert.AreEqual(OperationCode.PushConst, code[1].Code);
            Assert.AreEqual(0, code[1].Argument);
            Assert.AreEqual(OperationCode.Assign, code[2].Code);
        }
    }
}
