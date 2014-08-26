using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using System.Collections.Generic;

namespace OneScript.Tests
{
    [TestClass]
    public class Compiler_Factory_Tests
    {
        [TestMethod]
        public void Parser_Define_Variables_NoErrors()
        {
            string code = @"Перем А Экспорт;
                            Перем Б,В;";

            var builder = new TestCodeBuilder();
            var parser = new Parser(builder);

            Lexer lexer;
            PrepareParser(code, out lexer);

            Assert.IsTrue(parser.Build(new CompilerContext(), lexer));

            Assert.IsTrue(builder.Variables[0] == "АExport");
            Assert.IsTrue(builder.Variables[1] == "Б");
            Assert.IsTrue(builder.Variables[2] == "В");
            Assert.IsTrue(builder.Variables.Count == 3);

        }

        [TestMethod]
        public void Parser_Defines_Variables_And_Report_Errors()
        {
            string codeNoSemicolon = @"Перем А Экспорт
                            Перем Б,В;";
            string codeNoIdentifier1 = @"Перем А Экспорт
                            Перем ,В;";
            string codeNoIdentifierSkipToNext = @"Перем Экспорт
                            Перем ,В;";

            var builder = new TestCodeBuilder();
            var parser = new Parser(builder);

            Lexer lexer;
            PrepareParser(codeNoSemicolon, out lexer);

            List<string> errors = new List<string>();

            parser.CompilerError += (s, e) =>
                {
                    errors.Add(e.Exception.Message);
                    e.IsHandled = true;
                };

            bool buildSuccess = false;

            buildSuccess = parser.Build(new CompilerContext(), lexer);
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors[0].Contains("Ожидается символ ;"));
            Assert.IsFalse(buildSuccess);

            errors.Clear();
            PrepareParser(codeNoIdentifier1, out lexer);
            buildSuccess = parser.Build(new CompilerContext(), lexer);
            Assert.IsTrue(errors.Count == 2);
            Assert.IsTrue(errors[0].Contains("Ожидается символ ;"));
            Assert.IsTrue(errors[1].Contains("Ожидается идентификатор"));
            Assert.IsFalse(buildSuccess);

            errors.Clear();
            PrepareParser(codeNoIdentifierSkipToNext, out lexer);
            buildSuccess = parser.Build(new CompilerContext(), lexer);
            Assert.IsTrue(errors.Count == 2);
            Assert.IsTrue(errors[0].Contains("Ожидается идентификатор"));
            Assert.IsTrue(errors[1].Contains("Ожидается идентификатор"));
            Assert.IsFalse(buildSuccess);

        }

        private static void PrepareParser(string code, out Lexer lexer)
        {
            lexer = new Lexer();
            lexer.Code = code;
        }


    }

    class TestCodeBuilder : IModuleBuilder
    {
        
        public List<string> Variables { get; set; }

        public void BeginModule(CompilerContext context)
        {
            Variables = new List<string>();
        }

        public void DefineVariable(string name)
        {
            Variables.Add(name);
        }

        public void DefineExportVariable(string name)
        {
            Variables.Add(name + "Export");
        }

        public void CompleteModule()
        {

        }

        public void OnError(CompilerErrorEventArgs errorInfo)
        {

        }

        
    }
}
