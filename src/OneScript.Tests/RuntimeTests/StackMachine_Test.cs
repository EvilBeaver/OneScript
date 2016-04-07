using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime;
using OneScript.Language;
using OneScript.Core;

namespace OneScript.Tests.RuntimeTests
{
    [TestClass]
    public class StackMachine_Test
    {
        [TestMethod]
        public void Machine_Can_Run_Code_In_Process()
        {
            var machine = new OneScriptStackMachine();
            var engine = new OneScriptRuntime();
            engine.InjectSymbol("П", ValueFactory.Create());
            var process = engine.CreateProcess();

            var compiler = engine.GetCompilerService();
            var code = new StringCodeSource("П = 1");
            var module = compiler.CompileCodeBatch(code);

            machine.AttachTo(process);
            machine.SetCode(module);

            var thread = ScriptThread.Create(process);
            thread.Run(() =>
                {
                    machine.Run(0);
                    return ValueFactory.Create();
                });

            Assert.AreEqual(ValueFactory.Create(1), process.Memory[0].ValueOf("П"));
        }
    }
}
