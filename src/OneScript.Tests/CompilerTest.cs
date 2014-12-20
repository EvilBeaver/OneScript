using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting.Compiler;

namespace OneScript.Tests
{
    [TestClass]
    public class CompilerTest
    {
        [TestMethod]
        public void FactorySetsCorrectBuilderType()
        {
            var compiler = CompilerFactory<Builder>.Create();
            Assert.IsInstanceOfType(compiler.GetModuleBuilder(), typeof(Builder));
        }
    }
}
