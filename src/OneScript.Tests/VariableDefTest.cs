using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime;

namespace OneScript.Tests
{
    [TestClass]
    public class VariableDefTest
    {
        [TestMethod]
        public void VarIsExported()
        {
            var v = new VariableDef()
            {
                Name = "test"
            };

            v.Flags |= VariableFlags.Exported;
            Assert.IsTrue(v.IsExported);
        }

        [TestMethod]
        public void VarIsNotExported()
        {
            var v = new VariableDef()
            {
                Name = "test"
            };

            v.Flags |= VariableFlags.ModuleLevel;
            Assert.IsFalse(v.IsExported);
            Assert.IsTrue(v.Flags.HasFlag(VariableFlags.ModuleLevel));
        }

        [TestMethod]
        public void VarIsLocal()
        {
            var v = new VariableDef()
            {
                Name = "test"
            };
            v.Flags |= VariableFlags.Local;
            Assert.IsTrue(v.Flags.HasFlag(VariableFlags.Local));
        }

    }
}