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

        [TestMethod]
        public void Change_Value_By_Reference()
        {
            var v = new Variable(ValueFactory.Create(123));
            var reference = Reference.Create(v);

            Assert.AreSame(v.Value, reference.Value);
            reference.Value = ValueFactory.Create("hi");
            Assert.AreSame(v.Value, reference.Value);

        }

        [TestMethod]
        public void Dereferencing()
        {
            var v = new Variable();
            v.Value = ValueFactory.Create("1");
            var reference1 = Reference.Create(v);
            var reference2 = Reference.Create(reference1);
            var reference3 = Reference.Create(reference2);

            reference2.Value = ValueFactory.Create(123);
            Assert.IsTrue(v.Value.AsNumber() == 123);
            Assert.AreSame(v.Value, reference1.Value);

            var deref = reference2.Dereference();
            Assert.IsTrue(deref.AsNumber() == v.Value.AsNumber());
            reference2.Value = ValueFactory.Create("hi");
            Assert.IsFalse(deref == v.Value);
            Assert.AreNotSame(deref, v.Value);

            Assert.AreSame(reference3.Value, reference2.Value);
            deref = reference3.Dereference();
            Assert.IsTrue(deref.AsString() == v.Value.AsString());
            reference3.Value = ValueFactory.Create(false);
        }

        [TestMethod]
        public void ContextProperty_Reference()
        {
            IRuntimeContextInstance ctx = new ImportedMembersClass();
            int idx = ctx.FindProperty("BooleanProperty");

            var reference = Reference.Create(ctx, idx);
            reference.Value = ValueFactory.Create(true);

            Assert.IsTrue(((ImportedMembersClass)ctx).BooleanExplicitName == true);
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
