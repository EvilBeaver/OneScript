using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class Variables_Test
    {
        [TestMethod]
        public void New_Variable_Is_Undefined()
        {
            Variable v = new Variable();
            Assert.IsTrue(v.Type == BasicTypes.Undefined);
        }
        
        [TestMethod]
        public void Variable_Stores_And_Returns_Value()
        {
            IValue value = ValueFactory.Create(12345);
            IVariable variable = new Variable();

            variable.Value = value;
            Assert.AreEqual(variable.Value, value);
            variable.Value = ValueFactory.Create();
            Assert.AreNotEqual(variable.Value, value);

        }

        [TestMethod]
        public void Variable_Changes_Reference_Counter()
        {
            var counted = new RefCounter();
            bool disposed = false;
            IValue undefined = ValueFactory.Create();

            Variable v1 = new Variable(counted); // first increment
            Variable v2 = new Variable();

            counted.BeforeDisposal += (s, e) =>
                {
                    disposed = true;
                };

            Assert.AreNotEqual(v1, v2);

            v2.Value = counted; // second increment

            v1.Value = undefined; // release
            Assert.IsFalse(disposed);
            v2.Value = undefined; // release
            Assert.IsTrue(disposed);

        }

        class RefCounter : CounterBasedLifetimeService, IValue
        {
            public DataType Type
            {
                get { throw new NotImplementedException(); }
            }

            public double AsNumber()
            {
                throw new NotImplementedException();
            }

            public string AsString()
            {
                throw new NotImplementedException();
            }

            public DateTime AsDate()
            {
                throw new NotImplementedException();
            }

            public bool AsBoolean()
            {
                throw new NotImplementedException();
            }

            public IRuntimeContextInstance AsObject()
            {
                throw new NotImplementedException();
            }

            public bool Equals(IValue other)
            {
                throw new NotImplementedException();
            }

            public int CompareTo(IValue other)
            {
                throw new NotImplementedException();
            }
        }
    }
}
