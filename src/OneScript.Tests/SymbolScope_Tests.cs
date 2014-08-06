using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;

namespace OneScript.Tests
{
    [TestClass]
    public class SymbolScope_Tests
    {
        [TestMethod]
        public void SymbolScope_VariableRegistered()
        {
            var scope = new SymbolScope();
            int idx = scope.DefineVariable("NewMyVariable");
            int idx2 = scope.DefineVariable("NewMyVariable2");
            Assert.IsTrue(idx == 0);
            Assert.IsTrue(idx2 == 1);
            Assert.IsTrue(scope.IsVarDefined("NewMyVariable"));
            Assert.IsTrue(scope.GetVariableNumber("NewMyVariable") == 0);
            Assert.IsTrue(scope.IsVarDefined("NewMyVariable2"));
            Assert.IsTrue(scope.GetVariableNumber("NewMyVariable2") == 1);
            
            Assert.IsTrue(scope.GetVariableNumber("UnknownVar") == SymbolScope.InvalidIndex);
            Assert.IsTrue(scope.VariableCount == 2);
        }

        [TestMethod]
        public void SymbolScope_MethodRegistered()
        {
            var scope = new SymbolScope();
            int idx = scope.DefineMethod("NewMethod");
            int idx2 = scope.DefineMethod("NewMethod2");
            Assert.IsTrue(idx == 0);
            Assert.IsTrue(idx2 == 1);
            Assert.IsTrue(scope.IsMethodDefined("NewMethod"));
            Assert.IsTrue(scope.GetMethodNumber("NewMethod") == 0);
            Assert.IsTrue(scope.IsMethodDefined("NewMethod2"));
            Assert.IsTrue(scope.GetMethodNumber("NewMethod2") == 1);

            Assert.IsTrue(scope.GetMethodNumber("UnknownMethod") == SymbolScope.InvalidIndex);
            Assert.IsTrue(scope.MethodCount == 2);
        }

        [TestMethod]
        public void Invalid_Identifier_Throws_Exception()
        {
            var scope = new SymbolScope();

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => scope.DefineVariable("4 k44"), typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => scope.DefineMethod("4 k44"), typeof(ArgumentException)));
        }

        [TestMethod]
        public void Duplicate_Identifier_Throws_Exception()
        {
            var scope = new SymbolScope();
            scope.DefineVariable("myVar");
            scope.DefineMethod("myMethod");

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => scope.DefineVariable("MyVar"), typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => scope.DefineMethod("MyMethod"), typeof(ArgumentException)));
        }

        [TestMethod]
        public void CompilerContext_Variable_Registration()
        {
            var ctx = new CompilerContext();
            var scope1 = new SymbolScope();
            var scope2 = new SymbolScope();

            ctx.PushScope(scope1);
            var globalBind = ctx.DefineVariable("Global");
            Assert.IsTrue(globalBind.Name == "Global");
            Assert.IsTrue(globalBind.Context == 0);
            Assert.IsTrue(globalBind.IndexInContext == 0);
            Assert.AreSame(scope1, ctx.TopScope);
            Assert.IsTrue(scope1.IsVarDefined("Global"));

            ctx.PushScope(scope2);
            Assert.IsTrue(ctx.TopScope == scope2);

            var localBind = ctx.DefineVariable("Local");

            Assert.IsTrue(localBind.Name == "Local");
            Assert.IsTrue(localBind.Context == 1);
            Assert.IsTrue(localBind.IndexInContext == 0);
            Assert.IsTrue(scope2.IsVarDefined("Local"));

            Assert.IsTrue(ctx.IsVarDefined("Global"));
            Assert.IsTrue(ctx.IsVarDefined("Local"));
            Assert.AreEqual(ctx.GetVariable("Global"), globalBind);
            Assert.AreEqual(ctx.GetVariable("Local"), localBind);

            SymbolScope popped = ctx.PopScope();
            Assert.AreSame(scope2, popped);

            
        }

        [TestMethod]
        public void Duplicate_Var_Definition_Is_Possible_On_Different_Contexts()
        {
            var ctx = new CompilerContext();
            var scope1 = new SymbolScope();
            var scope2 = new SymbolScope();

            ctx.PushScope(scope1);
            ctx.DefineVariable("var1");
            ctx.DefineVariable("var2");
            ctx.PushScope(scope2);
            ctx.DefineVariable("var1");
            ctx.DefineVariable("var2");

        }

        [TestMethod]
        [ExpectedException(typeof(CompilerException))]
        public void Duplicate_Var_Definition()
        {
            var ctx = new CompilerContext();
            ctx.PushScope(new SymbolScope());
            ctx.DefineVariable("var1");
            ctx.DefineVariable("VAR1");
        }

        [TestMethod]
        public void CompilerContext_MethodRegistration()
        {
            var ctx = new CompilerContext();
            var scope1 = new SymbolScope();
            var scope2 = new SymbolScope();

            ctx.PushScope(scope1);
            var globalBind = ctx.DefineMethod("Global");
            Assert.IsTrue(globalBind.Name == "Global");
            Assert.IsTrue(globalBind.Context == 0);
            Assert.IsTrue(globalBind.IndexInContext == 0);
            Assert.AreSame(scope1, ctx.TopScope);
            Assert.IsTrue(scope1.IsMethodDefined("Global"));

            ctx.PushScope(scope2);
            Assert.IsTrue(ctx.TopScope == scope2);

            var localBind = ctx.DefineMethod("Local");

            Assert.IsTrue(localBind.Name == "Local");
            Assert.IsTrue(localBind.Context == 1);
            Assert.IsTrue(localBind.IndexInContext == 0);
            Assert.IsTrue(scope2.IsMethodDefined("Local"));

            Assert.IsTrue(ctx.IsMethodDefined("Global"));
            Assert.IsTrue(ctx.IsMethodDefined("Local"));
            Assert.AreEqual(ctx.GetMethod("Global"), globalBind);
            Assert.AreEqual(ctx.GetMethod("Local"), localBind);
        }
    }
}
