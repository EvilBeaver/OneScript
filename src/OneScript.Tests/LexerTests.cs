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
            Assert.IsTrue(lexer.Position == Lexer.OUT_OF_TEXT);
            Assert.IsTrue(lexer.CurrentLine == 1);
            lexer.NextLexem();
            Assert.IsTrue(lexer.Position >= 0);
            lexer.Code = "А = 1;";
            Assert.IsTrue(lexer.Position == Lexer.OUT_OF_TEXT);
        }

        [TestMethod]
        public void SourceCode_Iterator_Basics()
        {
            string code = "Б = 1;";
            var iterator = new SourceCodeIterator(code);
            
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == '\0');
            Assert.IsTrue(iterator.PeekNext() == 'Б');
            
            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == 'Б');
            Assert.IsTrue(iterator.PeekNext() == ' ');
            Assert.IsTrue(iterator.Position == 0);

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == ' ');
            Assert.IsTrue(iterator.PeekNext() == '=');
            Assert.IsTrue(iterator.Position == 1);

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == '=');
            Assert.IsTrue(iterator.PeekNext() == ' ');
            Assert.IsTrue(iterator.Position == 2);

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == ' ');
            Assert.IsTrue(iterator.PeekNext() == '1');
            Assert.IsTrue(iterator.Position == 3);

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == '1');
            Assert.IsTrue(iterator.PeekNext() == ';');
            Assert.IsTrue(iterator.Position == 4);

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentLine == 1);
            Assert.IsTrue(iterator.CurrentSymbol == ';');
            Assert.IsTrue(iterator.PeekNext() == '\0');
            Assert.IsTrue(iterator.Position == 5);

            Assert.IsFalse(iterator.MoveNext());
            Assert.IsTrue(iterator.CurrentSymbol == '\0');
            Assert.IsTrue(iterator.PeekNext() == '\0');
            

        }

        [TestMethod]
        public void Whitespace_Skipping()
        {
            string code = "Б \t\t=\n 1; ";
            string check = "Б=1;";
            var iterator = new SourceCodeIterator(code);

            for (int i = 0; i < check.Length; i++)
            {
                iterator.MoveToContent();
                Assert.IsTrue(check[i] == iterator.CurrentSymbol);
                iterator.MoveNext();
            }

            Assert.IsTrue(iterator.CurrentLine == 2);
            Assert.IsFalse(iterator.MoveNext());

        }

        [TestMethod]
        public void Retrieve_Line_Of_Code()
        {
            string code = @"А = 1;
            Б = 2;
            В = 7-11;
            Г = 8";

            var iterator = new SourceCodeIterator(code);
            while(iterator.CurrentLine<3)
            {
                if (!iterator.MoveNext())
                    Assert.Fail("Code is less than 3 lines");
            }

            Assert.IsTrue(iterator.CurrentLine == 3);
            Assert.IsTrue(iterator.CurrentSymbol == '\n');
            iterator.MoveNext();
            Assert.IsTrue(iterator.LineOfCode(3).Trim() == "В = 7-11;");

        }

        [TestMethod]
        public void CurrentColumn_Calculation()
        {
            string code = @"А = 1;
            Б = 2;
            В = 7-11;
            Г = 8";

            var iterator = new SourceCodeIterator(code);
            while (iterator.CurrentLine < 3)
            {
                if (!iterator.MoveNext())
                    Assert.Fail("Code is less than 3 lines");
            }

            Assert.IsTrue(iterator.CurrentLine == 3);
            iterator.MoveToContent();
            Assert.IsTrue(iterator.CurrentSymbol == 'В');
            Assert.IsTrue(iterator.CurrentColumn == 13);
        }

        [TestMethod]
        public void Identifier_LexerState_Works_Fine()
        {
            string code = "  \ndddddd-";
            var iterator = new SourceCodeIterator(code);
            var state = new WordLexerState();
            iterator.MoveToContent();
            var lexem = state.ReadNextLexem(iterator);
            Assert.IsTrue(lexem.Type == LexemType.Identifier);
            Assert.IsTrue(lexem.Content == "dddddd");
        }

        [TestMethod]
        public void StringLiteral_LexerState_WorksFine()
        {
            string code;
            SourceCodeIterator iterator;
            Lexem lex;
            StringLexerState state = new StringLexerState();
            
            code = " \"-just string \"";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.StringLiteral);
            Assert.IsTrue(lex.Content == "-just string ");

            code = @" ""-just
            |string """;
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.StringLiteral);
            Assert.IsTrue(lex.Content == "-just\r\nstring ");

            code = @" ""-just "" ""string"" ""123""";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.StringLiteral);
            Assert.IsTrue(lex.Content == "-just \nstring\n123");

            code = @"""first line
            |second line
            // comment
            |third line""";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.StringLiteral);
            Assert.IsTrue(lex.Content == "first line\r\nsecond line\r\nthird line");
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void Unclosed_String_Literal()
        {
            string code;
            SourceCodeIterator iterator;
            Lexem lex;
            StringLexerState state = new StringLexerState();

            code = " \"-just string ";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void Incorrect_NewLine_InString()
        {
            string code;
            SourceCodeIterator iterator;
            Lexem lex;
            StringLexerState state = new StringLexerState();

            code = @" ""-just 
            d|string """;
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
        }

        [TestMethod]
        public void NumberLiteral_State_Works_Fine()
        {
            string code = " 123.45 ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new NumberLexerState();
            var lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.NumberLiteral);
            Assert.IsTrue(lex.Content == "123.45");
        }

        [TestMethod]
        public void Wrong_Number_Literal()
        {
            string code = " 123.45.45 ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new NumberLexerState();

            Assert.IsTrue(TestHelpers.ExceptionThrown(() =>
                {
                    state.ReadNextLexem(iterator);
                },
                typeof(SyntaxErrorException)));


            code = " 12jk";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            Assert.IsTrue(TestHelpers.ExceptionThrown(() =>
                {
                    state.ReadNextLexem(iterator);
                },
                typeof(SyntaxErrorException)));

        }

        [TestMethod]
        public void Date_LexerState_Works_Fine()
        {
            string code = " '12341212' ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new DateLexerState();
            var lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.DateLiteral);
            Assert.IsTrue(lex.Content == "12341212");
        }

        [TestMethod]
        public void Operators_Lexer_State()
        {
            string code = " + - * / < > <= >= <> % ,.()[]";
            var iterator = new SourceCodeIterator(code);
            var state = new OperatorLexerState();

            Lexem lex;
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "+");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "-");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "*");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "/");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "<");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == ">");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "<=");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == ">=");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "<>");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "%");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == ",");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == ".");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "(");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == ")");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "[");

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.IsTrue(lex.Type == LexemType.Operator);
            Assert.IsTrue(lex.Content == "]");
        }
    }
}
