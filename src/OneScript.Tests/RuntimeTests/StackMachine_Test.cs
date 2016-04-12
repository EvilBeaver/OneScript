using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Runtime;
using OneScript.Language;
using OneScript.Core;
using System.Linq;
using OneScript.Runtime.Compiler;

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
                    var meth = module.Methods.First(x => x.Name == module.EntryPointName);
                    machine.Run(meth);
                    return ValueFactory.Create();
                });

            Assert.AreEqual(ValueFactory.Create(1), process.Memory[0].ValueOf("П"));
        }

        [TestMethod]
        public void Machine_Can_Enter_Method()
        {
            var machine = new OneScriptStackMachine();
            var builder = new OSByteCodeBuilder();
            builder.Context = new CompilerContext();
            var parser = new Parser(builder);
            var src = new Preprocessor();
            src.Code = "F = 1";
            parser.ParseModule(src);
            var module = builder.GetModule();

            machine.SetCode(module);
            machine.Enter(module.Methods[0]);
            Assert.IsTrue(machine.CurrentFrame.Module == module);
            Assert.IsTrue(machine.CurrentFrame.CurrentMethod == module.Methods[0].Name);

        }
    }
}
