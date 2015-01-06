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

        [TestMethod]
        public void Register_Module_Variable()
        {
            var builder = new ByteCodeModuleBuilder();
            SetBuilderContext(builder);
            builder.BeginModule();

            builder.DefineVariable("Тест");

            Assert.IsTrue(builder.SymbolsContext.IsVarDefined("Тест"));
            Assert.IsTrue(builder.Module.VariableRefs.Count == 1);
            Assert.IsTrue(builder.Module.VariableRefs[0].Context == 0);
            Assert.IsTrue(builder.Module.Variables.Count == 1);
            Assert.IsFalse(builder.Module.Variables[0].IsExported);

        }

        [TestMethod]
        public void Register_Module_ExportVariable()
        {
            var builder = new ByteCodeModuleBuilder();
            SetBuilderContext(builder);
            builder.BeginModule();

            builder.DefineExportVariable("Тест");

            Assert.IsTrue(builder.SymbolsContext.IsVarDefined("Тест"));
            Assert.IsTrue(builder.Module.VariableRefs.Count == 1);
            Assert.IsTrue(builder.Module.VariableRefs[0].Context == 0);
            Assert.IsTrue(builder.Module.Variables.Count == 1);
            Assert.IsTrue(builder.Module.Variables[0].IsExported);
        }

        [TestMethod]
        public void Add_Statements_To_Module_Body()
        {
            var builder = new ByteCodeModuleBuilder();
            SetBuilderContext(builder);
            builder.BeginModule();

            var one = new Lexem
            {
                Content = "1",
                Type = LexemType.NumberLiteral
            };

            builder.BeginModuleBody();
            builder.BeginBatch();
            builder.SelectOrUseVariable("Тест");
            builder.ReadLiteral(one);
            builder.BuildAssignment(null, null);
            builder.EndBatch(null);
            builder.CompleteModule();

            var module = builder.Module;
            Assert.IsTrue(module.VariableRefs.Count == 0);
            Assert.IsTrue(module.Variables.Count == 0);
            Assert.IsTrue(module.EntryMethodIndex == 0);
            Assert.IsTrue(module.Constants.Count == 1);
            Assert.IsTrue(module.Methods.Count == 1);
            Assert.IsTrue(module.Methods[0].Locals.Count == 1);
            Assert.IsTrue(module.Methods[0].Name == "$entry");

        }

        public void SetBuilderContext(ByteCodeModuleBuilder builder)
        {
            if (builder.SymbolsContext != null)
                throw new InvalidOperationException("Symbol scope is already set");

            builder.SymbolsContext = new CompilerContext();
        }
    }
}
