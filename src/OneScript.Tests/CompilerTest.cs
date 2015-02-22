using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Compiler;
using OneScript.Runtime;

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

        [TestMethod]
        public void Parameters_Are_Method_Locals()
        {
            var builder = PrepareBuilder();

            string code = "Процедура А(Б,В) Перем Г; КонецПроцедуры";

            var compiler = new CompilerEngine(builder);
            compiler.SetCode(code);
            Assert.IsTrue(compiler.Compile());

            var method = builder.Module.Methods[0];

            Assert.AreEqual(3, method.Locals.Count);
            foreach (var item in method.Locals)
            {
                Assert.IsTrue(item.IsLocal, "Variable " + item.Name + " must be local");
            }

            Assert.AreEqual("Б", method.Locals[0].Name);
            Assert.AreEqual("В", method.Locals[1].Name);
            Assert.AreEqual("Г", method.Locals[2].Name);

        }

        private static BCodeModuleBuilder PrepareBuilder()
        {
            var builder = new BCodeModuleBuilder();
            builder.SymbolsContext = new CompilerContext();
            builder.NewScope();
            return builder;
        }

        [TestMethod]
        public void Compiler_Creates_Methods()
        {
            string code = 
            @"Процедура А(Б,В)
                ; 
            КонецПроцедуры

            Функция В()

            КонецФункции";

            var builder = PrepareBuilder();
            var compiler = new CompilerEngine(builder);
            compiler.SetCode(code);

            compiler.Compile();

            var module = builder.Module;
            Assert.AreEqual(2, module.Methods.Count);
            Assert.AreEqual("А", module.Methods[0].Name);
            Assert.AreEqual("В", module.Methods[1].Name);

        }
    }
}
