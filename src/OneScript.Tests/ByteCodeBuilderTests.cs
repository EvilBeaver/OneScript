using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting.Compiler;
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
        public void ModuleBuilder_SymbolContexts_Access()
        {
            var builder = new ByteCodeModuleBuilder();

            Assert.IsNull(builder.SymbolsContext);
            builder.SymbolsContext = new CompilerContext();
            Assert.IsNotNull(builder.SymbolsContext);

            SymbolScope newScope = builder.NewScope();
            builder.SymbolsContext.DefineVariable("Тест");

            Assert.AreSame(newScope, builder.SymbolsContext.TopScope);
            Assert.AreSame(newScope, builder.ExitScope());
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

        [TestMethod]
        public void Use_Own_SymbolContext()
        {
            var builder = new ByteCodeModuleBuilder();
            Assert.IsNull(builder.SymbolsContext);
            SetBuilderContext(builder);
            Assert.IsNotNull(builder.SymbolsContext);
        }

        public void SetBuilderContext(ByteCodeModuleBuilder builder)
        {
            if (builder.SymbolsContext != null)
                throw new InvalidOperationException("Symbol scope is already set");

            builder.SymbolsContext = new CompilerContext();
        }
    }
}
