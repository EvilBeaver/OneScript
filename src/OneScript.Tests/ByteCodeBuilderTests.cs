using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting.Compiler;
using OneScript.Scripting.Runtime.CodeGeneration;
using OneScript.Scripting.Runtime;

namespace OneScript.Tests
{
    [TestClass]
    public class ByteCodeBuilderTests
    {
        [TestMethod]
        public void CompilerFactory_Creates_ByteCodeBuilder()
        {
            var compiler = CompilerFactory<ByteCodeModuleBuilder>.Create();
            Assert.IsInstanceOfType(compiler.GetModuleBuilder(), typeof(ByteCodeModuleBuilder));
        }

        [TestMethod]
        public void ModuleBuilder_Initialization()
        {
            var builder = new ByteCodeModuleBuilder();

            ModuleImage module = builder.Module;

            Assert.IsTrue(module.Code.Count == 0);
            Assert.IsTrue(module.Variables.Count == 0);
            Assert.IsTrue(module.Methods.Count == 0);

        }

        [TestMethod]
        public void ModuleBuilder_SymbolContexts_Access()
        {
            var builder = new ByteCodeModuleBuilder();

            Assert.IsNull(builder.SymbolsContext);
            builder.SymbolsContext = new CompilerContext();
            Assert.IsNotNull(builder.SymbolsContext);

            SymbolScope newScope = builder.NewScope();
            builder.SymbolsContext.DefineVariable("Тест");

            Assert.ReferenceEquals(newScope, builder.SymbolsContext.TopScope);
            Assert.ReferenceEquals(newScope, builder.ExitScope());
            Assert.IsTrue(newScope.IsVarDefined("Тест"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Empty_SymbolContext_Throws_OnNewScope()
        {
            var builder = new ByteCodeModuleBuilder();
            Assert.IsNull(builder.SymbolsContext);
            builder.NewScope();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Empty_SymbolContext_Throws_OnExitScope()
        {
            var builder = new ByteCodeModuleBuilder();
            Assert.IsNull(builder.SymbolsContext);
            builder.NewScope();
        }
    }
}
