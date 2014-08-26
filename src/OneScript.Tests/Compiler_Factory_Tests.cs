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

            Lexer lexer = PrepareParser(code);

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

            Lexer lexer = PrepareParser(codeNoSemicolon);
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
            lexer = PrepareParser(codeNoIdentifier1);
            buildSuccess = parser.Build(new CompilerContext(), lexer);
            Assert.IsTrue(errors.Count == 2);
            Assert.IsTrue(errors[0].Contains("Ожидается символ ;"));
            Assert.IsTrue(errors[1].Contains("Ожидается идентификатор"));
            Assert.IsFalse(buildSuccess);

            errors.Clear();
            lexer = PrepareParser(codeNoIdentifierSkipToNext);
            buildSuccess = parser.Build(new CompilerContext(), lexer);
            Assert.IsTrue(errors.Count == 2);
            Assert.IsTrue(errors[0].Contains("Ожидается идентификатор"));
            Assert.IsTrue(errors[1].Contains("Ожидается идентификатор"));
            Assert.IsFalse(buildSuccess);

        }

        [TestMethod]
        public void Simple_Assignment()
        {
            var builder = new TestCodeBuilder();
            var parser = new Parser(builder);
            Lexer lexer = PrepareParser("А = 2");
            
            var ctx = new CompilerContext();
            
            Assert.IsTrue(parser.Build(ctx, lexer));

            Assert.IsTrue(builder.EntryPoint != -1);
            Assert.IsTrue(builder.Constants[0].Presentation == "2");

        }

        private static Lexer PrepareParser(string code)
        {
            var lexer = new Lexer();
            lexer.Code = code;
            return lexer;
        }


    }

    class TestCodeBuilder : IModuleBuilder
    {
        
        public List<string> Variables { get; set; }

        public List<ConstDefinition> Constants { get; set; }

        public int EntryPoint { get; set; }

        #region IBuilder members
        public void BeginModule(ICompilerContext context)
        {
            Variables = new List<string>();
            Constants = new List<ConstDefinition>();
            EntryPoint = -1;
        }

        public void BuildVariable(string name)
        {
            Variables.Add(name);
        }

        public void BuildExportVariable(string name)
        {
            Variables.Add(name + "Export");
        }

        public void BuildLoadVariable(SymbolBinding binding)
        {
            EntryPoint++;
        }

        public void BuildReadConstant(ConstDefinition constDef)
        {
            Constants.Add(constDef);
            EntryPoint++;
        }

        public void CompleteModule()
        {

        }

        public void OnError(CompilerErrorEventArgs errorInfo)
        {

        } 
        
        public void SnapToCodeLine(int line)
        {
            throw new NotImplementedException();
        }

        public void BuildGetReference(ConstDefinition constDef)
        {
            throw new NotImplementedException();
        }

        public void WriteReference()
        {
            throw new NotImplementedException();
        }

        public void BuildBinaryOperation(Token operationToken)
        {
            throw new NotImplementedException();
        }

        public void BuildUnaryOperation(Token operationToken)
        {
            throw new NotImplementedException();
        }

        public void BuildMethodCall(string methodName, int argumentCount, bool asFunction)
        {
            throw new NotImplementedException();
        }

        public void BuildAssignment()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
