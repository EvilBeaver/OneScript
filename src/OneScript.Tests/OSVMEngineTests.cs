using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime;

namespace OneScript.Tests
{
    [TestClass]
    public class OSVMEngineTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Import_Of_Conflicting_Contexts_Is_Not_Allowed()
        {
            var engine = new OSVMEngine();

            var ctx1 = new ImportedMembersClass();
            var ctx2 = new ImportedMembersClass();

            engine.InjectObject(ctx1);
            engine.InjectObject(ctx2);
        }
    }
}
