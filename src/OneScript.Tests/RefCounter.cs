using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class RefCounter
    {
        [TestMethod]
        public void Test_AutoDispose()
        {
            var ad = new AutoDisposeStub();
            Assert.IsFalse(ad.Disposed);
            Assert.IsTrue(ad.AddRef() == 1);
            Assert.IsTrue(ad.AddRef() == 2);
            Assert.IsTrue(ad.Release() == 1);
            Assert.IsTrue(ad.Release() == 0);
            Assert.IsTrue(ad.Disposed);
        }

        class AutoDisposeStub : IRefCountable
        {
            CounterBasedLifetimeService _counter;

            public AutoDisposeStub()
            {
                _counter = new CounterBasedLifetimeService();
                _counter.BeforeDisposal += (s, e) => Dispose();
            }

            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
            }

            public int AddRef()
            {
                return _counter.AddRef();
            }

            public int Release()
            {
                return _counter.Release();
            }
        }

    }
}
