using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using OneScript;

namespace OneScript.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Empty_Lexer_Position_Is_Negative()
        {
            var lexer = new Lexer();
            Assert.IsTrue(lexer.Position == Lexer.OUT_OF_TEXT);
            Assert.IsTrue(lexer.CurrentLine == Lexer.OUT_OF_TEXT);
            
        }

        [TestMethod]
        public void Code_Set_Sets_Position()
        {
            var lexer = new Lexer();
            lexer.Code = "А = 1;";
            Assert.IsTrue(lexer.Code == "А = 1;");
            Assert.IsTrue(lexer.Position == 0);
            Assert.IsTrue(lexer.CurrentLine == 0);
            //lexer.NextLexem();
            //Assert.IsTrue(lexer.Position > 0);
            //lexer.Code = "А = 1;";
            //Assert.IsTrue(lexer.Position == 0);
        }

        [TestMethod]
        public void SourceCode_Iterator_Basics()
        {
            var emptyIterator = new SourceCodeIterator("");
            Assert.IsTrue(emptyIterator.CurrentLine == Lexer.OUT_OF_TEXT);
            Assert.IsTrue(emptyIterator.CurrentSymbol == '\0');

            string code = "Б = 1;";
            var iterator = new SourceCodeIterator(code);
            
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == 'Б');
            Assert.IsTrue(iterator.PeekNext() == '=');
            
            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.MoveToContent());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == '=');
            Assert.IsTrue(iterator.PeekNext() == '1');

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.MoveToContent());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == '1');
            Assert.IsTrue(iterator.PeekNext() == ';');

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.MoveToContent());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == ';');
            Assert.IsTrue(iterator.PeekNext() == '\0');

            Assert.IsFalse(iterator.MoveNext());
            Assert.IsFalse(iterator.MoveToContent());
            Assert.IsTrue(iterator.CurrentSymbol == '\0');
            

        }
    }
}
