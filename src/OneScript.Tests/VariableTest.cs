using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;
using OneScript.Runtime;

namespace OneScript.Tests
{
    [TestClass]
    public class VariableTest
    {
        [TestMethod]
        public void New_Variable_Has_Undefined_Type()
        {
            Variable testVar = new Variable();
            Assert.IsTrue(testVar.Value.Type == BasicTypes.Undefined);
        }

        [TestMethod]
        public void VariableScope_All_Are_Undefined()
        {
            Variable[] vars = Variable.CreateArray(5);
            for (int i = 0; i < vars.Length; i++)
			{
                Assert.IsTrue(vars[i].Value.Type == BasicTypes.Undefined);
			}
        }

        [TestMethod]
        public void Variable_Accepts_Value()
        {
            var v = Variable.Create(ValueFactory.Create(100));
            Assert.IsTrue(v.Value.AsNumber() == 100m);
            v.Value = ValueFactory.Create("HI");
            Assert.IsTrue(v.Value.Type == BasicTypes.String);
            Assert.IsTrue(v.Value.AsString() == "HI");
        }
    }
}
