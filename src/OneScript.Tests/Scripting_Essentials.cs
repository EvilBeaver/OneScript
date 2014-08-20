using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class Scripting_Essentials
    {
        [TestMethod]
        public void ParametersList_Test()
        {
            var param = new MethodParameter[2];
            param[0] = new MethodParameter();
            param[1] = new MethodParameter();

            var pl = new ParametersList(param);

            Assert.IsTrue(pl.Count == 2);
            Assert.IsTrue(pl[0].Equals(param[0]));
            Assert.IsTrue(pl[1].Equals(param[1]));

            var list2 = ParametersList.CreateDefault(2);
            for (int i = 0; i < 2; i++)
            {
                Assert.IsFalse(list2[i].IsByValue);
                Assert.IsFalse(list2[i].IsOptional);
            }

        }

        [TestMethod]
        public void MethodUsageData_Creation()
        {
            var proc = MethodSignatureData.CreateProcedure(ParametersList.CreateDefault(1));
            var func = MethodSignatureData.CreateFunction(ParametersList.CreateDefault(2));

            Assert.IsTrue(func.IsFunction);
            Assert.IsFalse(proc.IsFunction);

            Assert.IsTrue(proc.Parameters.Count == 1);
            Assert.IsTrue(func.Parameters.Count == 2);

            proc = MethodSignatureData.CreateProcedure(1);
            func = MethodSignatureData.CreateFunction(2);

            Assert.IsTrue(func.IsFunction);
            Assert.IsFalse(proc.IsFunction);

            Assert.IsTrue(proc.Parameters.Count == 1);
            Assert.IsTrue(func.Parameters.Count == 2);
        }

        [TestMethod]
        public void IsValidIdentifier()
        {
            Assert.IsTrue(Utils.IsValidIdentifier("Var"));
            Assert.IsTrue(Utils.IsValidIdentifier("Var123"));
            Assert.IsTrue(Utils.IsValidIdentifier("Var_123"));
            Assert.IsTrue(Utils.IsValidIdentifier("_Var"));
            Assert.IsFalse(Utils.IsValidIdentifier("123Var"));
            Assert.IsFalse(Utils.IsValidIdentifier("V a r"));
            Assert.IsFalse(Utils.IsValidIdentifier("Var$"));
        }

        [TestMethod]
        public void Extract_MethodData_From_Context()
        {
            var ctx = new ImportedMembersClass();
            var data = MethodSignatureExtractor.Extract(ctx);

            Assert.IsTrue(data.Length == 3);
            Assert.IsFalse(data[0].IsFunction);
            Assert.IsTrue(data[1].IsFunction);
            Assert.IsTrue(data[2].IsFunction);
            Assert.IsTrue(data[2].Parameters.Count == 3);
            
            Assert.IsTrue(data[2].Parameters[0].IsByValue);
            Assert.IsFalse(data[2].Parameters[0].IsOptional);
            
            Assert.IsTrue(data[2].Parameters[1].IsByValue);
            Assert.IsFalse(data[2].Parameters[1].IsOptional);

            Assert.IsTrue(data[2].Parameters[2].IsByValue);
            Assert.IsTrue(data[2].Parameters[2].IsOptional);

        }

        [TestMethod]
        public void Attached_Memory_From_Context()
        {
            var ctx = new ImportedMembersClass();
            ctx.ReadOnlyString = "hello";
            ctx.IntProperty = 1;
            ctx.BooleanAutoName = true;
            ctx.BooleanExplicitName = true;

            var memBlock = MemoryAttachedContext.CreateFromContext(ctx);

            Assert.IsTrue(memBlock.ContextInstance == ctx);
            int idx = ctx.FindProperty("IntProperty");
            Assert.IsTrue(memBlock.State[idx].Equals(ValueFactory.Create(1)));
            
            idx = ctx.FindProperty("ReadOnlystring");
            Assert.IsTrue(memBlock.State[idx].Equals(ValueFactory.Create("hello")));

            idx = ctx.FindProperty("BooleanProperty");
            Assert.IsTrue(memBlock.State[idx].Equals(ValueFactory.Create(true)));

            idx = ctx.FindProperty("BooleanAutoName");
            Assert.IsTrue(memBlock.State[idx].Equals(ValueFactory.Create(true)));

            idx = ctx.FindMethod("Func");
            Assert.IsTrue(memBlock.Methods[idx].IsFunction);
            Assert.IsTrue(memBlock.Methods[idx].Parameters.Count == 3);
            Assert.IsTrue(memBlock.Methods[idx].Parameters[2].IsOptional);
            
        }

    }
}
