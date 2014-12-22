using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using System.Text;
using OneScript.Scripting.Compiler;
using OneScript.Scripting.Compiler.Lexics;

namespace OneScript.Tests
{
    [TestClass]
    public class PreprocessorTests
    {
        [TestMethod]
        public void Definitions_Are_Defined()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            pp.Define("ВебКлиент");
            Assert.IsTrue(pp.IsDefined("сервер"));
            Assert.IsTrue(pp.IsDefined("вебКлиеНт"));
            Assert.IsFalse(pp.IsDefined("client"));
            pp.Undef("ВебКлиент");
            Assert.IsFalse(pp.IsDefined("ВебКлиент"));
        }

        [TestMethod]
        public void Preprocessor_If_True()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            pp.Define("Клиент");

            var code = @"
            #Если Сервер и Клиент Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.AreEqual(LexemType.Identifier, lex.Type);
            Assert.AreEqual("F", lex.Content);

            lex = pp.NextLexem();
            Assert.AreEqual(LexemType.EndOperator, lex.Type);

            lex = pp.NextLexem();
            Assert.AreEqual(LexemType.EndOfText, lex.Type);
        }

        [TestMethod]
        public void Preprocessor_If_False()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            
            var code = @"
            #Если Сервер и Клиент Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.AreEqual(Token.EndOfText, lex.Token);
            Assert.AreEqual(LexemType.EndOfText, lex.Type);

        }

        [TestMethod]
        public void Preprocessof_IfElse()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            pp.Define("Клиент");
            pp.Define("ВебКлиент");

            var code = @"
            #Если Сервер и Не Клиент Тогда
                А
            #ИначеЕсли ВебКлиент Тогда
                Б
            #ИначеЕсли СовсемНеКлиент Тогда
                Х
            #Иначе
                В
            #КонецЕсли";

            pp.Code = code;

            Lexem lex;
            lex = pp.NextLexem();
            Assert.IsTrue(lex.Content == "Б");
            lex = pp.NextLexem();
            
            Assert.AreEqual(Token.EndOfText, lex.Token);
            Assert.AreEqual(LexemType.EndOfText, lex.Type);
        }

        [TestMethod]
        public void Preprocessor_CompositeTest()
        {
            var pp = new Preprocessor();
            
            string code = @"
            #Если Клиент Тогда
                К
            #ИначеЕсли Сервер или ВебКлиент Тогда
                СВ
            #ИначеЕсли ТолстыйКлиент и Не ВебКлиент Тогда
                ТнВ
            #Иначе
                И
            #КонецЕсли";

            pp.Define("Сервер");
            Assert.IsTrue(GetPreprocessedContent(pp, code) == "СВ");
            pp.Define("Клиент");
            Assert.IsTrue(GetPreprocessedContent(pp, code) == "К");
            pp.Undef("Сервер");
            pp.Undef("Клиент");
            pp.Define("ВебКлиент");
            Assert.IsTrue(GetPreprocessedContent(pp, code) == "СВ");
            pp.Define("ТолстыйКлиент");
            pp.Undef("ВебКлиент");            
            Assert.IsTrue(GetPreprocessedContent(pp, code) == "ТнВ");
            pp.Undef("ТолстыйКлиент");
            Assert.IsTrue(GetPreprocessedContent(pp, code) == "И");


        }

        [TestMethod]
        public void Folded_Preprocessor_Items()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            pp.Define("Клиент");
            pp.Define("ВебКлиент");

            var code = @"
            #Если Сервер Тогда
                #Если ВебКлиент Тогда
                    Б
                #КонецЕсли
                Х
            #Иначе
                В
            #КонецЕсли";

            var preprocessed = GetPreprocessedContent(pp, code);

            Assert.AreEqual("БХВ", preprocessed);

        }

        private string GetPreprocessedContent(Preprocessor pp, string code)
        {
            pp.Code = code;
            Lexem lex = Lexem.Empty();

            StringBuilder builder = new StringBuilder();

            while (lex.Type != LexemType.EndOfText)
            {

                lex = pp.NextLexem();

                builder.Append(lex.Content);

            }
            return builder.ToString().Trim();
        }
    }
}
