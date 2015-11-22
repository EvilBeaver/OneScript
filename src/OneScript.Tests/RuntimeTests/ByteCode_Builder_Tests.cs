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
            return CreateModuleForCode(codeString, rt);
        }

        private static CompiledModule CreateModuleForCode(string codeString, AbstractScriptRuntime rt)
        {
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
            var b = builder.BeginModuleBody();
            Assert.AreEqual(1, ctx.TopScopeIndex);
            builder.EndModuleBody(b);
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
            builder.EndModuleBody(builder.BeginModuleBody());
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

        [TestMethod]
        public void Method_Entry_Points_Are_Assigned()
        {
            var builder = new OSByteCodeBuilder();
            var ctx = new CompilerContext();
            builder.Context = ctx;
            builder.BeginModule();

            builder.ReadLiteral(new Lexem() { Content = "1", Type = LexemType.NumberLiteral });
            builder.ReadLiteral(new Lexem() { Content = "2", Type = LexemType.NumberLiteral });

            var ast = builder.BeginMethod();
            ast.Parameters = new[]
                {
                    new ASTMethodParameter(){IsOptional = true, DefaultValueLiteral = new ConstDefinition(){Type = ConstType.Undefined}}
                };
            ast.Identifier = "test";
            builder.EndMethod(ast);
            builder.CompleteModule();
            var module = builder.GetModule();
            Assert.AreEqual(2, module.Methods[0].EntryPoint);
        }

        [TestMethod]
        public void Body_Entry_Point_Is_Assigned()
        {
            var builder = new OSByteCodeBuilder();
            var ctx = new CompilerContext();
            builder.Context = ctx;
            builder.BeginModule();

            builder.ReadLiteral(new Lexem() { Content = "1", Type = LexemType.NumberLiteral });
            builder.ReadLiteral(new Lexem() { Content = "2", Type = LexemType.NumberLiteral });

            builder.EndModuleBody(builder.BeginModuleBody());
            
            builder.CompleteModule();
            var module = builder.GetModule();
            Assert.AreEqual(2, module.Methods[0].EntryPoint);
        }

        [TestMethod]
        public void Local_Variables_Stored_In_Method()
        {
            var code = CreateModuleForCode("Процедура А() Б = 2; В = 4; КонецПроцедуры");
            var varmap = code.Methods[0].VariableTable;

            Assert.AreEqual(2, varmap.Count);
            Assert.AreEqual(2, varmap[0].Context); // external + module + local = 2
            Assert.AreEqual(2, varmap[1].Context);
            Assert.AreEqual(0, varmap[0].IndexInContext);
            Assert.AreEqual(1, varmap[1].IndexInContext);
        }

        [TestMethod]
        public void ASTModuleNode_Creates_MethodDef_With_Parameters()
        {
            var code = CreateModuleForCode("Процедура А(Знач Первый, Второй, Знач Третий = 1) ; КонецПроцедуры");

            Assert.AreEqual(1, code.Methods.Count);
            var method = code.Methods[0];

            Assert.AreEqual(3, method.Parameters.Length);
            Assert.AreEqual("Первый", method.Parameters[0].Identifier);
            Assert.IsTrue(method.Parameters[0].IsByValue);
            Assert.IsFalse(method.Parameters[0].IsOptional);
            Assert.AreEqual(CompiledModule.InvalidEntityIndex, method.Parameters[0].DefaultValueIndex);

            Assert.AreEqual("Второй", method.Parameters[1].Identifier);
            Assert.IsFalse(method.Parameters[1].IsByValue);
            Assert.IsFalse(method.Parameters[1].IsOptional);
            Assert.AreEqual(CompiledModule.InvalidEntityIndex, method.Parameters[1].DefaultValueIndex);

            Assert.AreEqual("Третий", method.Parameters[2].Identifier);
            Assert.IsTrue(method.Parameters[2].IsByValue);
            Assert.IsTrue(method.Parameters[2].IsOptional);
            Assert.AreEqual(0, method.Parameters[2].DefaultValueIndex);
            Assert.AreEqual("1", code.Constants[0].Presentation);
            Assert.AreEqual(ConstType.Number, code.Constants[0].Type);
        }

        [TestMethod]
        public void MethodCall_Creates_A_Usage_Record()
        {
            var rt = new OneScriptRuntime();
            var methodsProvider = new ImportedMembersClass();
            rt.InjectObject(methodsProvider);

            var code = CreateModuleForCode("Proc();", rt);

            Assert.AreEqual(1, code.Methods.Count); //тело модуля
            Assert.AreEqual(1, code.MethodUsageMap.Count);
            Assert.AreEqual(1, code.MethodUsageMap[0].Context);
            Assert.AreEqual("Proc", code.MethodUsageMap[0].Name);
        }
    }
}
