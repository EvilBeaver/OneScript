/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using Xunit;

namespace OneScript.Language.Tests
{
    public class LexerTests
    {
        [Fact]
        public void Empty_Lexer_Position_Is_Negative()
        {
            var lexer = new DefaultLexer();
            Assert.True(lexer.Iterator.CurrentColumn == ErrorPositionInfo.OUT_OF_TEXT);
            Assert.True(lexer.Iterator.CurrentLine == ErrorPositionInfo.OUT_OF_TEXT);

        }

        [Fact]
        public void SourceCode_Iterator_Basics()
        {
            string code = "Б = 1;";
            var iterator = new SourceCodeIterator(code);

            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == '\0');
            Assert.True(iterator.PeekNext() == 'Б');

            Assert.True(iterator.MoveNext());
            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == 'Б');
            Assert.True(iterator.PeekNext() == ' ');
            Assert.True(iterator.Position == 0);

            Assert.True(iterator.MoveNext());
            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == ' ');
            Assert.True(iterator.PeekNext() == '=');
            Assert.True(iterator.Position == 1);

            Assert.True(iterator.MoveNext());
            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == '=');
            Assert.True(iterator.PeekNext() == ' ');
            Assert.True(iterator.Position == 2);

            Assert.True(iterator.MoveNext());
            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == ' ');
            Assert.True(iterator.PeekNext() == '1');
            Assert.True(iterator.Position == 3);

            Assert.True(iterator.MoveNext());
            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == '1');
            Assert.True(iterator.PeekNext() == ';');
            Assert.True(iterator.Position == 4);

            Assert.True(iterator.MoveNext());
            Assert.True(iterator.CurrentLine == 1);
            Assert.True(iterator.CurrentSymbol == ';');
            Assert.True(iterator.PeekNext() == '\0');
            Assert.True(iterator.Position == 5);

            Assert.False(iterator.MoveNext());
            Assert.True(iterator.CurrentSymbol == '\0');
            Assert.True(iterator.PeekNext() == '\0');


        }

        [Fact]
        public void Whitespace_Skipping()
        {
            string code = "Б \t\t=\n 1; ";
            string check = "Б=1;";
            var iterator = new SourceCodeIterator(code);

            for (int i = 0; i < check.Length; i++)
            {
                iterator.MoveToContent();
                Assert.True(check[i] == iterator.CurrentSymbol);
                iterator.MoveNext();
            }

            Assert.True(iterator.CurrentLine == 2);
            Assert.False(iterator.MoveNext());

        }

        [Fact]
        public void Retrieve_Line_Of_Code()
        {
            string code = @"А = 1;
            Б = 2;
            // comment
            В = 7-11;
            Г = 8";

            var iterator = new SourceCodeIterator(code);
            while(iterator.CurrentLine<4)
            {
                if (!iterator.MoveNext())
                    throw new Xunit.Sdk.XunitException("Code is less than 4 lines");
            }

            Assert.True(iterator.CurrentLine == 4);
            Assert.True(iterator.CurrentSymbol == '\n');
            iterator.MoveNext();
            Assert.True(iterator.GetCodeLine(4).Trim() == "В = 7-11;");

        }

        [Fact]
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
                    throw new Xunit.Sdk.XunitException("Code is less than 3 lines");
            }

            Assert.True(iterator.CurrentLine == 3);
            iterator.MoveToContent();
            Assert.True(iterator.CurrentSymbol == 'В');
            Assert.True(iterator.CurrentColumn == 13);
        }

        [Fact]
        public void Identifier_LexerState_Works_Fine()
        {
            string code = "  \ndddddd-";
            var iterator = new SourceCodeIterator(code);
            var state = new WordLexerState();
            iterator.MoveToContent();
            var lexem = state.ReadNextLexem(iterator);
            Assert.True(lexem.Type == LexemType.Identifier);
            Assert.True(lexem.Content == "dddddd");
        }

        [Fact]
        public void Word_Lexer_State_BuiltIn_Tokens_As_Usual_Words()
        {
            string code = "Лев СтрДлина Прав";
            var iterator = new SourceCodeIterator(code);
            var state = new WordLexerState();
            iterator.MoveToContent();
            var lexem = state.ReadNextLexem(iterator);
            Assert.True(lexem.Type == LexemType.Identifier);
            Assert.True(lexem.Content == "Лев");
            Assert.True(lexem.Token == Token.NotAToken);

            iterator.MoveToContent();
            lexem = state.ReadNextLexem(iterator);
            Assert.True(lexem.Type == LexemType.Identifier);
            Assert.True(lexem.Content == "СтрДлина");
            Assert.True(lexem.Token == Token.NotAToken);

            iterator.MoveToContent();
            lexem = state.ReadNextLexem(iterator);
            Assert.True(lexem.Type == LexemType.Identifier);
            Assert.True(lexem.Content == "Прав");
            Assert.True(lexem.Token == Token.NotAToken);
        }

        [Fact]
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
            Assert.True(lex.Type == LexemType.StringLiteral);
            Assert.Equal("-just string ", lex.Content);

            code = @" ""-just
            |string """;
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.StringLiteral);
            Assert.Equal("-just\nstring ", lex.Content);

            code = @" ""-just "" ""string"" ""123""";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.StringLiteral);
            Assert.Equal("-just \nstring\n123", lex.Content);

            code = @"""first line
            |second line
            // comment
            |third line""";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.StringLiteral);
            Assert.Equal("first line\nsecond line\nthird line", lex.Content);
        }

        [Fact]
        public void Word_Literals_Processed_Correctly()
        {
            string code;
            SourceCodeIterator iterator;
            Lexem lex;
            WordLexerState state = new WordLexerState();

            code = " Истина  Ложь  Неопределено  Null  True False Undefined";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.Equal(LexemType.BooleanLiteral, lex.Type);
            Assert.Equal("Истина", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.BooleanLiteral);
            Assert.Equal("Ложь", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.UndefinedLiteral);
            Assert.Equal("Неопределено", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.NullLiteral);
            Assert.Equal("Null", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.BooleanLiteral);
            Assert.Equal("True", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.BooleanLiteral);
            Assert.Equal("False", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.UndefinedLiteral);
            Assert.Equal("Undefined", lex.Content);

        }

        [Fact]
        public void Preprocessor_Lexem_ProcessedCorrectly()
        {
            string code = @"#Если
                #КонецЕсли";

            var iterator = new SourceCodeIterator(code);
            var wordParser = new PreprocessorDirectiveLexerState();
            Lexem lex;

            iterator.MoveToContent();
            lex = wordParser.ReadNextLexem(iterator);
            Assert.Equal(LexemType.PreprocessorDirective, lex.Type);
            Assert.Equal("Если", lex.Content);
            Assert.Equal(Token.If, lex.Token);

            iterator.MoveToContent();
            lex = wordParser.ReadNextLexem(iterator);
            Assert.Equal(LexemType.PreprocessorDirective, lex.Type);
            Assert.Equal("КонецЕсли", lex.Content);
            Assert.Equal(Token.EndIf, lex.Token);

        }

        [Fact]
        public void Unclosed_String_Literal()
        {
            string code;
            SourceCodeIterator iterator;
            StringLexerState state = new StringLexerState();

            code = " \"-just string ";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            Assert.Throws<SyntaxErrorException>(() => state.ReadNextLexem(iterator));
        }

        [Fact]
        public void Incorrect_NewLine_InString()
        {
            string code;
            SourceCodeIterator iterator;
            StringLexerState state = new StringLexerState();

            code = @" ""-just 
            d|string """;
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            Assert.Throws<SyntaxErrorException>(() => state.ReadNextLexem(iterator));
        }

        [Fact]
        public void NumberLiteral_State_Works_Fine()
        {
            string code = " 123.45 ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new NumberLexerState();
            var lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.NumberLiteral);
            Assert.Equal("123.45", lex.Content);
        }

        [Fact]
        public void Wrong_Number_Literal()
        {
            string code = " 123.45.45 ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new NumberLexerState();

            Assert.Throws<SyntaxErrorException>(() => state.ReadNextLexem(iterator));

            code = " 12jk";
            iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            Assert.Throws<SyntaxErrorException>(() => state.ReadNextLexem(iterator));

        }

        [Fact]
        public void Date_LexerState_Works_With_8_Numbers()
        {
            string code = " '12341212' ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new DateLexerState();
            var lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.DateLiteral);
            Assert.Equal("12341212", lex.Content);
        }

        [Fact]
        public void Date_LexerState_Works_With_14_Numbers()
        {
            string code = " '12341212020202' ";
            var iterator = new SourceCodeIterator(code);
            iterator.MoveToContent();
            var state = new DateLexerState();
            var lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.DateLiteral);
            Assert.Equal("12341212020202", lex.Content);
        }

        [Fact]
        public void Operators_Lexer_State()
        {
            string code = " + - * / < > <= >= <> % ,.()[]";
            var iterator = new SourceCodeIterator(code);
            var state = new OperatorLexerState();

            Lexem lex;
            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("+", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("-", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("*", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("/", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("<", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal(">", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("<=", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal(">=", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("<>", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("%", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal(",", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal(".", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("(", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal(")", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("[", lex.Content);

            iterator.MoveToContent();
            lex = state.ReadNextLexem(iterator);
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("]", lex.Content);
        }

        [Fact]
        public void NonWhiteSpaceLexer_Reads_UnquotedStrings()
        {
            var code = "AA.BB()--Data Second";
            
            var lb = new LexerBuilder();
            lb.Detect((cs,i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            var lexer = lb.Build();

            lexer.Iterator = new SourceCodeIterator(code);
            
            var lex = lexer.NextLexem();
            
            Assert.Equal(LexemType.NotALexem, lex.Type);
            Assert.Equal(Token.NotAToken, lex.Token);
            Assert.Equal("AA.BB()--Data", lex.Content);

            lex = lexer.NextLexem();
            Assert.Equal("Second", lex.Content);

            lex = lexer.NextLexem();
            Assert.Equal(Lexem.EndOfText(), lex);
        }
        
        [Fact]
        public void NonWhiteSpaceLexer_Discards_Comments()
        {
            var code = "AA.BB()-// comment";
            
            var lb = new LexerBuilder();
            lb.Detect((cs,i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            var lexer = lb.Build();

            lexer.Iterator = new SourceCodeIterator(code);
            
            var lex = lexer.NextLexem();
            
            Assert.Equal(LexemType.NotALexem, lex.Type);
            Assert.Equal(Token.NotAToken, lex.Token);
            Assert.Equal("AA.BB()-", lex.Content);

            lex = lexer.NextLexem();
            Assert.Equal(Lexem.EndOfText(), lex);
        }
        
        [Fact]
        public void NonWhiteSpaceLexer_Reads_QuotedStrings()
        {
            var code = @"""Quoted space //and comment"" next";
            
            var lb = new LexerBuilder();
            lb.Detect((cs,i) => !char.IsWhiteSpace(cs))
                .HandleWith(new NonWhitespaceLexerState());

            var lexer = lb.Build();

            lexer.Iterator = new SourceCodeIterator(code);
            var lex = lexer.NextLexem();
            Assert.Equal("\"Quoted space //and comment\"", lex.Content);
            
            lex = lexer.NextLexem();
            Assert.Equal("next", lex.Content);
            
            lex = lexer.NextLexem();
            Assert.Equal(Lexem.EndOfText(), lex);
        }
        
        [Fact]
        public void Code_Walkthrough()
        {
            string code = @"
            А = Б+11.2 <> 
            '20100207' - ""ffff""";

            var lexer = new DefaultLexer();
            lexer.Iterator = new SourceCodeIterator(code);

            Lexem lex;
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.Identifier);
            Assert.Equal("А", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("=", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.Identifier);
            Assert.Equal("Б", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("+", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.NumberLiteral);
            Assert.Equal("11.2", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("<>", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.DateLiteral);
            Assert.Equal("20100207", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.Operator);
            Assert.Equal("-", lex.Content);
            lex = lexer.NextLexem();
            Assert.True(lex.Type == LexemType.StringLiteral);
            Assert.Equal("ffff", lex.Content);
        }

        [Fact]
        public void Syntax_Error_Handling()
        {
            string code = @"
            А$Б";

            var lexer = new DefaultLexer();
            lexer.Iterator = new SourceCodeIterator(code);
            lexer.UnexpectedCharacterFound += (s, e) =>
                {
                    e.Iterator.MoveNext();
                    e.IsHandled = true;
                };

            Lexem lex = lexer.NextLexem();
            Assert.Equal("А", lex.Content);
            lex = lexer.NextLexem();
            Assert.Equal("Б", lex.Content);
        }

        [Fact]
        public void New_Exception_Shows_Negative_Line_And_Column()
        {
            var e = new ScriptException();
            Assert.True(e.LineNumber == -1);
            Assert.True(e.ColumnNumber == -1);
        }

        [Fact]
        public void Comments_Are_Retrieved_Correctly()
        {
            string code = "а //comment\r\n// another comment";
            var lexer = GetLexerForCode(code);
            Lexem lex;

            lexer.NextLexem();
            lex = lexer.NextLexem();

            Assert.Equal(LexemType.Comment, lex.Type);
            Assert.Equal(Token.NotAToken, lex.Token);
            Assert.Equal("//comment", lex.Content);

            lex = lexer.NextLexem();
            Assert.Equal(LexemType.Comment, lex.Type);
            Assert.Equal(Token.NotAToken, lex.Token);
            Assert.Equal("// another comment", lex.Content);

        }

        [Fact]
        public void Lexer_Ignores_Comments()
        {
            string code = "a //comment\r\n// another comment\r\nvalue";
            var lexer = new DefaultLexer();
            lexer.Iterator = new SourceCodeIterator(code);
            Lexem lex;

            lex = lexer.NextLexem();

            Assert.Equal(LexemType.Identifier, lex.Type);
            Assert.Equal("a", lex.Content);

            lex = lexer.NextLexem();
            Assert.Equal(LexemType.Identifier, lex.Type);
            Assert.Equal("value", lex.Content);
        }
        
        private ILexer GetLexerForCode(string code)
        {
            return new FullSourceLexer
            {
                Iterator = new SourceCodeIterator(code)
            };
        }
    }
}
