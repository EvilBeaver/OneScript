using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
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
        public void Use_Variable_Creates_Reference()
        {
            var builder = GetBuilder();
            builder.DefineVariable("test");
            builder.BeginModuleBody();
            builder.SelectOrUseVariable("test");
            builder.SelectOrUseVariable("test2");
            var moduleRefs = builder.Module.VariableRefs;
            var locals = builder.Module.Methods[0].Locals;
            
            Assert.AreEqual("test", moduleRefs[0].Name);
            Assert.AreEqual(1, locals.Count);
            Assert.AreEqual("test2", locals[0].Name);
        }

        [TestMethod]
        public void Use_Variable_Creates_Code_PushVar()
        {
            var builder = GetBuilder();
            builder.DefineVariable("test");
            builder.DefineVariable("test2");
            builder.BeginModuleBody();
            builder.SelectOrUseVariable("test2");
            builder.SelectOrUseVariable("test");
            var compiledModule = builder.Module;
            Assert.IsTrue(compiledModule.EntryMethodIndex == 0);
            Assert.IsTrue(compiledModule.Methods[0].EntryPoint == 0);
            Assert.AreEqual(OperationCode.PushVar, compiledModule.Code[0].Code);
            Assert.AreEqual(0, compiledModule.Code[0].Argument);
            Assert.AreEqual(OperationCode.PushVar, compiledModule.Code[1].Code);
            Assert.AreEqual(1, compiledModule.Code[1].Argument);
        }

        [TestMethod]
        public void ReadingConstant_Creates_Const_Record()
        {
            var builder = GetBuilder();
            builder.BeginModuleBody();
            builder.ReadLiteral(new Lexem()
            {
                Type = LexemType.NumberLiteral,
                Content = "123"
            });

            var module = builder.Module;
            Assert.AreEqual(1, module.Constants.Count);
            Assert.AreEqual(ConstType.Number, module.Constants[0].Type);
            Assert.AreEqual("123", module.Constants[0].Presentation);
            Assert.AreEqual(OperationCode.PushConst, module.Code[0].Code);
        }

        [TestMethod]
        [ExpectedException(typeof(CompilerException))]
        public void ReadVariable_Throws_When_Var_Undefined()
        {
            var builder = GetBuilder();
            builder.ReadVariable("test");
        }

        [TestMethod]
        public void Read_Variable_Creates_Code_PushVar()
        {
            var builder = GetBuilder();
            builder.DefineVariable("test");
            builder.DefineVariable("test2");
            builder.BeginModuleBody();
            builder.ReadVariable("test2");
            builder.ReadVariable("test");
            var compiledModule = builder.Module;
            Assert.IsTrue(compiledModule.EntryMethodIndex == 0);
            Assert.IsTrue(compiledModule.Methods[0].EntryPoint == 0);
            Assert.AreEqual(OperationCode.PushVar, compiledModule.Code[0].Code);
            Assert.AreEqual(0, compiledModule.Code[0].Argument);
            Assert.AreEqual(OperationCode.PushVar, compiledModule.Code[1].Code);
            Assert.AreEqual(1, compiledModule.Code[1].Argument);
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
