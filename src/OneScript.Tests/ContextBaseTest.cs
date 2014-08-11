using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.ComponentModel;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class ContextBaseTest
    {
        [TestMethod]
        public void Context_DefaultBehavior()
        {
            var ctx = new CtxTest();
            Assert.AreSame(ctx, ctx.AsObject());
            Assert.IsTrue(ctx.IsIndexed);
            Assert.IsTrue(ctx.GetPropCount() == 0);
            Assert.IsTrue(ctx.GetMethodsCount() == 0);
            
            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.GetPropertyName(0),
                typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.GetPropertyValue(0),
                typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.SetPropertyValue(0, ValueFactory.Create()),
                typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.GetMethodName(0),
                typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.GetParametersCount(0),
                typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.HasReturnValue(0),
                typeof(ArgumentException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => 
                    {
                        IValue dummy;
                        ctx.GetDefaultValue(0, 0, out dummy);
                    },
                typeof(ArgumentException)));

            Assert.IsFalse(ctx.DynamicMethodSignatures);
        }

        [TestMethod]
        public void Acces_Properties_By_Index()
        {
            var ctx = new CtxTest2();
            Assert.IsTrue(ctx.GetPropCount() == 1);

            int index = ctx.FindProperty("HelLoStRing");
            Assert.IsTrue(index == 0);

            var indexVal = ValueFactory.Create("HelloString");
            Assert.IsTrue(ctx.GetIndexedValue(indexVal).Type == BasicTypes.String);
            Assert.IsTrue(ctx.GetIndexedValue(indexVal).AsString() == "");
            ctx.SetIndexedValue(indexVal, ValueFactory.Create("hello"));
            Assert.IsTrue(ctx.GetIndexedValue(indexVal).AsString() == "hello");
        }

        class CtxTest : ContextBase
        {

        }

        class CtxTest2 : ContextBase
        {
            string _hsValue = "";
            public override int GetPropCount()
            {
                return 1;
            }

            public override bool IsPropReadable(int index)
            {
                return true;
            }
            public override bool IsPropWriteable(int index)
            {
                return true;
            }
            protected override IValue GetPropertyValueInternal(int index)
            {
                if (index == 0)
                    return ValueFactory.Create(_hsValue);
                else
                    return base.GetPropertyValue(index);
            }
            protected override void SetPropertyValueInternal(int index, IValue newValue)
            {
                if (index == 0)
                    _hsValue = newValue.AsString();
                else
                    base.SetPropertyValue(index, newValue);
            }
            public override int FindProperty(string name)
            {
                if (name.ToLower() == "hellostring")
                    return 0;
                else
                    return base.FindProperty(name);
            }

            public override string GetPropertyName(int index)
            {
                if (index == 0)
                    return "HelloString";
                else
                    return base.GetPropertyName(index);

            }
        }
    }
}
