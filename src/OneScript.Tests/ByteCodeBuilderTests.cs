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
            var compiler = CompilerFactory<BCodeModuleBuilder>.Create();
            Assert.IsInstanceOfType(compiler.GetModuleBuilder(), typeof(BCodeModuleBuilder));
        }

        [TestMethod]
        public void ModuleBuilder_SymbolContexts_Access()
        {
            var builder = new BCodeModuleBuilder();

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
            var builder = new BCodeModuleBuilder();
            Assert.IsNull(builder.SymbolsContext);
            builder.NewScope();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Empty_SymbolContext_Throws_OnExitScope()
        {
            var builder = new BCodeModuleBuilder();
            Assert.IsNull(builder.SymbolsContext);
            builder.NewScope();
        }

        [TestMethod]
        public void Use_Own_SymbolContext()
        {
            var builder = new BCodeModuleBuilder();
            Assert.IsNull(builder.SymbolsContext);
            SetBuilderContext(builder);
            Assert.IsNotNull(builder.SymbolsContext);
        }

        [TestMethod]
        public void NewBuilder_Creates_Empty_Module()
        {
            var builder = new BCodeModuleBuilder();
            Assert.IsNull(builder.Module);
            SetBuilderContext(builder);
            builder.BeginModule();
            Assert.AreEqual(0, builder.Module.Variables.Count);
            Assert.AreEqual(0, builder.Module.Methods.Count);
            Assert.AreEqual(0, builder.Module.Code.Count);
        }

        [TestMethod]
        public void Define_Module_Variable()
        {
            var builder = GetBuilder();
            builder.DefineVariable("test");

            Assert.AreEqual(1, builder.Module.Variables.Count);
            Assert.AreEqual(0, builder.Module.VariableRefs.Count);
            Assert.AreEqual("test", builder.Module.Variables[0].Name);
            Assert.IsFalse(builder.Module.Variables[0].IsExported);
        }

        [TestMethod]
        public void Define_Module_Export_Variable()
        {
            var builder = GetBuilder();
            builder.DefineExportVariable("test");

            Assert.AreEqual(1, builder.Module.Variables.Count);
            Assert.AreEqual(0, builder.Module.VariableRefs.Count);
            Assert.AreEqual("test", builder.Module.Variables[0].Name);
            Assert.IsTrue(builder.Module.Variables[0].IsExported);
        }

        [TestMethod]
        public void Module_Body_Entry_Method_Created()
        {
            var builder = GetBuilder();
            builder.BeginModuleBody();
            Assert.AreEqual(1, builder.Module.Methods.Count);
            Assert.AreEqual("$entry", builder.Module.Methods[0].Name);
        }

        [TestMethod]
        public void Use_Module_Variable_Creates_Reference()
        {
            var builder = GetBuilder();
            builder.BeginModuleBody();
            builder.SelectOrUseVariable("test");
            var refs = builder.Module.Methods[0].VariableRefs;

            Assert.AreEqual("test", refs[0].Name);
        }

        private BCodeModuleBuilder GetBuilder()
        {
            var builder = new BCodeModuleBuilder();
            SetBuilderContext(builder);
            builder.BeginModule();
            return builder;
        }

        public void SetBuilderContext(BCodeModuleBuilder builder)
        {
            if (builder.SymbolsContext != null)
                throw new InvalidOperationException("Symbol scope is already set");

            builder.SymbolsContext = new CompilerContext();
        }
    }
}
