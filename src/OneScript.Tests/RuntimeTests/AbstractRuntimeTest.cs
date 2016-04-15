using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime;
using OneScript.Core;

namespace OneScript.Tests.RuntimeTests
{
    [TestClass]
    public class AbstractRuntimeTest
    {
        [TestMethod]
        public void Abstract_Runtime_API_Can_Execute_Simple_Code()
        {
            IScriptRuntime rt = new OneScriptRuntime();
            IScriptSource src = new StringCodeSource("А = 1");

            var module = rt.Compile(src);
            rt.Execute(module, module.EntryPointName);
        }
    }
}
