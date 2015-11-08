using System;
using OneScript.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime.Compiler;
using OneScript.Language;

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
            Assert.AreEqual(OperationCode.PushLocal, code[0].Code);
            Assert.AreEqual(OperationCode.PushConst, code[1].Code);
            Assert.AreEqual(0, code[1].Argument);
            Assert.AreEqual(OperationCode.Assign, code[2].Code);
        }

        [TestMethod]
        public void Variables_And_Constants_Are_Numerated()
        {
            var module = CreateModuleForCode("а = 1;б = 2;");
            var code = module.Commands;

            Assert.AreEqual(6, code.Count);
            Assert.AreEqual(OperationCode.PushLocal, code[0].Code);
            Assert.AreEqual(0, code[0].Argument);
            Assert.AreEqual(OperationCode.PushConst, code[1].Code);
            Assert.AreEqual(0, code[1].Argument);

            Assert.AreEqual(OperationCode.PushLocal, code[3].Code);
            Assert.AreEqual(1, code[3].Argument);
            Assert.AreEqual(OperationCode.PushConst, code[4].Code);
            Assert.AreEqual(1, code[4].Argument);

            Assert.AreEqual(2, module.Constants.Count);
            //Assert.AreEqual(2, module.VariableTable.Count);
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
            builder.BeginModuleBody();
            Assert.AreEqual(1, ctx.TopScopeIndex);
            builder.EndModuleBody();
            Assert.AreEqual(0, ctx.TopScopeIndex);
            builder.CompleteModule();
            Assert.AreEqual(-1, ctx.TopScopeIndex);
        }

        [TestMethod]
        public void ModuleBody_Adds_Method()
        {
            var builder = new OSByteCodeBuilder();
            var ctx = new CompilerContext();
            builder.Context = ctx;
            builder.BeginModule();
            builder.BeginModuleBody();
            builder.EndModuleBody();
            var module = builder.GetModule();
            Assert.AreEqual(1, module.Methods.Count);
            Assert.AreEqual("$entry", module.EntryPointName);

        }

        [TestMethod]
        public void Default_Method_Param_Adds_A_Constant()
        {
            var builder = new OSByteCodeBuilder();
            var ctx = new CompilerContext();
            builder.Context = ctx;
            builder.BeginModule();
            var ast = builder.BeginMethod();
            ast.Parameters = new[]
                {
                    new ASTMethodParameter(){IsOptional = true, DefaultValueLiteral = new ConstDefinition(){Type = ConstType.Undefined}}
                };
            ast.Identifier = "test";
            builder.EndMethod(ast);
            builder.CompleteModule();
            var module = builder.GetModule();

            Assert.AreEqual(1, module.Constants.Count);
            Assert.AreEqual(1, module.Methods.Count);
        }
    }
}
