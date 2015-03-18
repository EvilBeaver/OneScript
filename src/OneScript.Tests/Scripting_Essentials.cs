using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;
using OneScript.Runtime;
using OneScript.ComponentModel;

namespace OneScript.Tests
{
    [TestClass]
    public class Scripting_Essentials
    {
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
            Assert.IsFalse(Utils.IsValidIdentifier(null));

        }

    }
}
