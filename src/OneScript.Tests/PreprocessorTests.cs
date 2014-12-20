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

            var iterator = new SourceCodeIterator(@"
            #Если Сервер и Клиент Тогда
                F;
            #КонецЕсли");
            iterator.MoveToContent();
            Assert.IsTrue(pp.Solve(iterator));
            iterator.MoveToContent();
            Assert.IsTrue(iterator.CurrentSymbol == 'F');
            iterator.MoveNext();
            iterator.MoveNext();
            iterator.MoveToContent();
            Assert.IsTrue(pp.Solve(iterator));
            Assert.IsFalse(iterator.MoveNext());
            pp.End();
        }

        [TestMethod]
        public void Preprocessor_If_False()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            pp.Define("Клиент");

            var iterator = new SourceCodeIterator(@"
            #Если Сервер и Не Клиент Тогда
                F;
            #КонецЕсли");
            iterator.MoveToContent();
            Assert.IsFalse(pp.Solve(iterator));
            iterator.MoveToContent();
            while (iterator.CurrentSymbol != '#')
            {
                iterator.MoveNext();
            }
            iterator.MoveToContent();
            Assert.IsTrue(pp.Solve(iterator));
            Assert.IsFalse(iterator.MoveNext());
            pp.End();
        }

        [TestMethod]
        public void Preprocessof_IfElse()
        {
            var pp = new Preprocessor();
            pp.Define("Сервер");
            pp.Define("Клиент");
            pp.Define("ВебКлиент");

            var iterator = new SourceCodeIterator(@"
            #Если Сервер и Не Клиент Тогда
                А
            #ИначеЕсли ВебКлиент Тогда
                Б
            #ИначеЕсли СовсемНеКлиент Тогда
                Х
            #Иначе
                В
            #КонецЕсли");
            iterator.MoveToContent();
            Assert.IsFalse(pp.Solve(iterator));
            Assert.IsTrue(iterator.CurrentSymbol == 'А');
            iterator.MoveNext();
            iterator.MoveToContent();
            Assert.IsTrue(pp.Solve(iterator));
            Assert.IsTrue(iterator.CurrentSymbol == 'Б');
            iterator.MoveNext();
            iterator.MoveToContent();
            Assert.IsFalse(pp.Solve(iterator));
            Assert.IsTrue(iterator.CurrentSymbol == 'Х');
            iterator.MoveNext();
            iterator.MoveToContent();
            Assert.IsFalse(pp.Solve(iterator));
            Assert.IsTrue(iterator.CurrentSymbol == 'В');
            iterator.MoveNext();
            iterator.MoveToContent();
            Assert.IsTrue(pp.Solve(iterator));
            pp.End();
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
        public void Preprocessable_Lexer_Skips_Lexems_Correctly()
        {
            string code = @"
            F = 1;
            #Если Клиент Тогда
                К = 1; // cccc
                M = 2;
            #КонецЕсли
            X = 0;";

            Lexer lexer = new Lexer();
            lexer.Code = code;

            Lexem lex = lexer.NextLexem();
            Assert.IsTrue(lex.Content == "F");
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Content == "=" && lex.Type == LexemType.Operator);
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Type == LexemType.NumberLiteral);
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Token == Token.Semicolon && lex.Type == LexemType.EndOperator);
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Content == "X" && lex.Type == LexemType.Identifier);
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Content == "=" && lex.Type == LexemType.Operator);
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Type == LexemType.NumberLiteral);
            lex = lexer.NextLexem();
            Assert.IsTrue(lex.Token == Token.Semicolon && lex.Type == LexemType.EndOperator);
        }

        private string GetPreprocessedContent(Preprocessor pp, string code)
        {
            var iterator = new SourceCodeIterator(code);

            var builder = new StringBuilder();
            while (iterator.MoveToContent())
            {
                if (iterator.CurrentSymbol == '#')
                {
                    if (pp.Solve(iterator))
                    {
                        while (iterator.CurrentSymbol != '#')
                        {
                            if (Char.IsLetterOrDigit(iterator.CurrentSymbol))
                                builder.Append(iterator.CurrentSymbol);
                            
                            if (!iterator.MoveNext())
                                break;
                        }
                        iterator.MoveToContent();
                    }
                }
                else
                {
                    iterator.MoveNext();
                }
            }
            return builder.ToString().Trim();
        }
    }
}
