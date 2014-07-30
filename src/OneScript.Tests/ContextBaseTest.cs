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

        class CtxTest : ContextBase
        {

        }
    }
}
