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
        public void Machine_Accepts_Module_And_Memory_Ref()
        {
            var machine = new OneScriptStackMachine();
            var memory  = new MachineMemory();
            
            var builder = new OSByteCodeBuilder();
            var parser = new Parser(builder);
            var lexer = new FullSourceLexer();
            lexer.Code = "П = 1";
            parser.ParseCodeBatch(lexer);

            var module = builder.GetModule();

            machine.AttachTo(memory);
            machine.SetCode(module);
            machine.Run(0);

            Assert.AreEqual(ValueFactory.Create(1), memory[0].ValueOf("П"));
        }
    }
}
