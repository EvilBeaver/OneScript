using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;
using OneScript.Runtime;

namespace OneScript.Tests.RuntimeTests
{
    [TestClass]
    public class MemoryManager_Test
    {
        [TestMethod]
        public void RuntimeScope_Can_Be_Created_From_Context()
        {
            var context = new ImportedMembersClass();
            var scope = RuntimeScope.FromContext(context);

            Assert.AreEqual(context.FindProperty("ЧисловоеЗначение"), scope.GetVariableNumber("ЧисловоеЗначение"));
            Assert.AreEqual(context.FindProperty("IntProperty"), scope.GetVariableNumber("IntProperty"));

            Assert.AreEqual(context.FindProperty("БулевоСвойство"), scope.GetVariableNumber("БулевоСвойство"));
            Assert.AreEqual(context.FindProperty("BooleanProperty"), scope.GetVariableNumber("BooleanProperty"));

            Assert.AreEqual(context.FindProperty("ReadOnlyString"), scope.GetVariableNumber("ReadOnlyString"));

            Assert.AreEqual(context.FindMethod("Процедура"), scope.GetMethodNumber("Процедура"));
            Assert.AreEqual(context.FindMethod("Proc"), scope.GetMethodNumber("Proc"));

            Assert.AreEqual(context.FindMethod("Функция"), scope.GetMethodNumber("Функция"));
            Assert.AreEqual(context.FindMethod("Func"), scope.GetMethodNumber("Func"));

        }

        [TestMethod]
        public void RuntimeScope_Gets_Value_ByName()
        {
            var context = new ImportedMembersClass();
            context.BooleanExplicitName = true;

            var scope = RuntimeScope.FromContext(context);
            Assert.AreEqual(ValueFactory.Create(true), scope.ValueOf("БулевоСвойство"));
        }

        [TestMethod]
        public void RuntimeScope_Gets_Value_By_Index()
        {
            var context = new ImportedMembersClass();
            context.BooleanExplicitName = true;

            var scope = RuntimeScope.FromContext(context);
            var index = context.FindProperty("БулевоСвойство");
            Assert.AreEqual(ValueFactory.Create(true), scope.ValueOf(index));
            
        }

        [TestMethod]
        public void Runtime_Scope_Values_In_Object_And_In_Scope_Are_Linked()
        {
            var context = new ImportedMembersClass();
            context.BooleanExplicitName = true;

            var scope = RuntimeScope.FromContext(context);
            var index = context.FindProperty("БулевоСвойство");

            Assert.AreEqual(ValueFactory.Create(true), scope.ValueOf(index));
            // присвоили значение переменной
            scope.ValueRefs[index].Value = ValueFactory.Create(false);
            // убедились, что в объекте значение поменялось
            Assert.AreEqual(ValueFactory.Create(false), context.GetPropertyValue(index));
        }

        [TestMethod]
        public void Runtime_Scope_Implements_ISymbolScope()
        {
            var context = new ImportedMembersClass();
            var scope = RuntimeScope.FromContext(context);

            Assert.IsInstanceOfType(scope, typeof(ISymbolScope));

            Assert.AreEqual(scope.VariableCount, 4);
            Assert.AreEqual(scope.MethodCount, 3);
            Assert.IsTrue(scope.IsVarDefined("БулевоСвойство"));
            Assert.IsTrue(scope.IsMethodDefined("Func"));
            Assert.IsTrue(scope.IsMethodDefined("Функция"));
        }

        [TestMethod]
        public void Machine_Memory_Accepts_Scopes()
        {
            var context = new ImportedMembersClass();
            context.BooleanExplicitName = true;

            var scope1 = RuntimeScope.FromContext(context);
            var scope2 = RuntimeScope.FromContext(context);
            var mem = new MachineMemory();

            mem.AddScope(scope1);
            mem.PushScope(scope2);
            Assert.AreSame(scope1, mem[0]);
            Assert.AreSame(scope2, mem[1]);
            Assert.AreEqual(2, mem.ScopeCount);
            Assert.AreEqual(1, mem.FixedScopeCount);
            mem.PopScope();
            Assert.AreEqual(1, mem.ScopeCount);
            Assert.AreEqual(1, mem.FixedScopeCount);
        }
    }
}
