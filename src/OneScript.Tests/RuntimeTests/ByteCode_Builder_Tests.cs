using System;
using OneScript.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scopes;

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

        [TestMethod]
        public void Start_Of_Module_Appends_Scope()
        {
            var builder = new OSByteCodeBuilder();
            builder.Context = new CompilerContext();

            builder.BeginModule();
            Assert.AreEqual(0, builder.Context.TopScopeIndex);
            builder.CompleteModule();
            Assert.AreEqual(-1, builder.Context.TopScopeIndex);
        }

        [TestMethod]
        public void Scoping_Is_Correct()
        {
            var builder = new OSByteCodeBuilder();
            var ctx = new CompilerContext();
            builder.Context = ctx;
            builder.BeginModule();
            Assert.AreEqual(0, ctx.TopScopeIndex);
            var n = builder.BeginMethod();
            Assert.AreEqual(1, ctx.TopScopeIndex);
            builder.EndMethod(n);
            Assert.AreEqual(0, ctx.TopScopeIndex);
            builder.CompleteModule();
            Assert.AreEqual(-1, ctx.TopScopeIndex);
        }
    }
}
