/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;
using Moq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using Xunit;

namespace OneScript.Language.Tests
{
   public class PreprocessorTests
    {
        [Fact]
        public void Definitions_Are_Defined()
        {
            var pp = new ConditionalDirectiveHandler(Mock.Of<IErrorSink>());
            pp.Define("Сервер");
            pp.Define("ВебКлиент");
            Assert.True(pp.IsDefined("сервер"));
            Assert.True(pp.IsDefined("вебКлиеНт"));
            Assert.False(pp.IsDefined("client"));
            pp.Undef("ВебКлиент");
            Assert.False(pp.IsDefined("ВебКлиент"));
        }

        [Fact]
        public void PreprocessingLexer_If_True()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");
            pp.Define("Клиент");

            var code = @"
            #Если Сервер и Клиент Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.Equal(LexemType.Identifier, lex.Type);
            Assert.Equal("F", lex.Content);

            lex = pp.NextLexem();
            Assert.Equal(LexemType.EndOperator, lex.Type);

            lex = pp.NextLexem();
            Assert.Equal(LexemType.EndOfText, lex.Type);
        }

        [Fact]
        public void PreprocessingLexer_If_False()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");

            var code = @"
            #Если Сервер и Клиент Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.Equal(Token.EndOfText, lex.Token);
            Assert.Equal(LexemType.EndOfText, lex.Type);

        }

        [Fact]
        public void PreprocessingLexer_IfElse()
        {
            var pp = new PreprocessingLexer();
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
            Assert.True(lex.Content == "Б");
            lex = pp.NextLexem();

            Assert.Equal(Token.EndOfText, lex.Token);
            Assert.Equal(LexemType.EndOfText, lex.Type);
        }

        [Fact]
        public void PreprocessingLexer_CompositeTest()
        {
            var pp = new PreprocessingLexer();

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
            Assert.Equal("СВ", GetPreprocessedContent(pp, code));
            pp.Define("Клиент");
            Assert.True(GetPreprocessedContent(pp, code) == "К");
            pp.Undef("Сервер");
            pp.Undef("Клиент");
            pp.Define("ВебКлиент");
            Assert.True(GetPreprocessedContent(pp, code) == "СВ");
            pp.Define("ТолстыйКлиент");
            pp.Undef("ВебКлиент");
            Assert.True(GetPreprocessedContent(pp, code) == "ТнВ");
            pp.Undef("ТолстыйКлиент");
            Assert.True(GetPreprocessedContent(pp, code) == "И");


        }

        [Fact]
        public void Folded_PreprocessingLexer_Items()
        {
            var pp = new PreprocessingLexer();
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

            Assert.Equal("БХ", preprocessed);

            pp.Undef("ВебКлиент");
            preprocessed = GetPreprocessedContent(pp, code);
            Assert.Equal("Х", preprocessed);

            pp.Undef("Сервер");
            preprocessed = GetPreprocessedContent(pp, code);
            Assert.Equal("В", preprocessed);
        }

        [Fact]
        public void PreprocessingLexer_Branching()
        {
            string code = @"
            Привет,
            #Если ТыВидишь Тогда
            ты видишь
                #Если Картошка Тогда
                    картошку
                #ИначеЕсли Морковка Тогда
                    морковку
                #ИначеЕсли Капуста Тогда
                    капусту
                #Иначе
                    шоколадку
                #КонецЕсли
            тогда ты молодец
            #Иначе
            тут ничего нет
            #КонецЕсли";

            var pp = new PreprocessingLexer();

            var preprocessed = GetPreprocessedContent(pp, code);
            Assert.Equal("Привет,тутничегонет", preprocessed);

            pp.Define("ТыВидишь");
            preprocessed = GetPreprocessedContent(pp, code);
            Assert.Equal("Привет,тывидишьшоколадкутогдатымолодец", preprocessed);

            pp.Define("Морковка");
            preprocessed = GetPreprocessedContent(pp, code);
            Assert.Equal("Привет,тывидишьморковкутогдатымолодец", preprocessed);
        }

        [Fact]
        public void PreprocessingLexer_Unclosed_IfBlock()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");

            var code = @"
            #Если Сервер и Клиент Тогда
                F;
            ";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_IfElse_Without_If()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");

            var code = @"
            #ИначеЕсли Сервер Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_ExpressionInterrupted()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");

            var code = @"
            #Если Сервер ИЛИ Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }
        
        [Fact]
        public void PreprocessingLexer_ExpressionUnexpected()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");

            var code = @"
            #Если Сервер ИЛИ + Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_Else_Without_If()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Сервер");

            var code = @"
            #Иначе
                F;
            #КонецЕсли";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PriorityOperators()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #Если Нет и СовсемНет ИЛИ Да Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.Equal("F", lex.Content);
        }

        [Fact]
        public void PriorityOperators_WithParenthesis()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #Если Нет и (Да или Да) Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.Equal(Token.EndOfText, lex.Token);
        }
        
        [Fact]
        public void ParsingFirstNot()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #Если Не Да Тогда
                F;
            #КонецЕсли";

            pp.Code = code;

            var lex = pp.NextLexem();

            Assert.Equal(Token.EndOfText, lex.Token);
        }

        [Fact]
        public void PreprocessingLexer_Unclosed_ElseBlock()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #Если Да и Не Да Тогда
                F;
            #Иначе
                G;
            ";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => {while (pp.NextLexem().Token != Token.EndOfText);});
        }

        [Fact]
        public void PreprocessingLexer_Endif_Without_If()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #КонецЕсли
                H;
            ";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_Extra_Endif()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #Если Да Тогда
                F;
            #КонецЕсли
            #КонецЕсли
            ";

            pp.Code = code;

            Assert.Throws<SyntaxErrorException>(() => {while (pp.NextLexem().Token != Token.EndOfText);});
        }

        [Fact]
        public void PreprocessingLexer_SimpleRegion()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg1
            
            #КонецОбласти
            F";

            pp.Code = code;
            var lex = pp.NextLexem();
            Assert.Equal("F", lex.Content);
        }

        [Fact]
        public void PreprocessingLexer_MultipleNestedRegions()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Region reg1
            #Область reg2
            
            #Область if // keywords are ok
            
            #endRegion
            #КонецОбласти // reg 1
            
            #endRegion
            # Область  reg1 // same name is ok

            #КонецОбласти
            F";

            pp.Code = code;
            var lex = pp.NextLexem();
            Assert.Equal("F", lex.Content);
        }


        [Fact]
        public void PreprocessingLexer_NoEndRegion()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg1
            #Область reg2
            #КонецОбласти
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => { while (pp.NextLexem().Token != Token.EndOfText) ; });
        }

        [Fact]
        public void PreprocessingLexer_ExtraEndRegion()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg1
            #КонецОбласти
            #КонецОбласти
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_BadRegionName()
        {
            var pp = new RegionDirectiveHandler(new ThrowingErrorSink());
            var lexer = new DefaultLexer();

            var code = @"
            #Область -reg
            #КонецОбласти
            F";

            lexer.Iterator = new SourceCodeIterator(code);
            var lexem = lexer.NextLexem();
            
            Assert.Throws<SyntaxErrorException>(() => pp.HandleDirective(ref lexem, lexer));
        }

        [Fact]
        public void PreprocessingLexer_NoRegionName()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область
            #КонецОбласти
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_NoRegionNameWithComment()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область // no name
            #КонецОбласти
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_SymbolsAfterName()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg 00
            #КонецОбласти
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_SymbolsAfterEndRegion()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg
            #КонецОбласти reg
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => pp.NextLexem());
        }

        [Fact]
        public void PreprocessingLexer_DirectiveAfterLineBreak()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg
            #

            КонецОбласти
            F";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => { while (pp.NextLexem().Token != Token.EndOfText) ; });
        }

        [Fact]
        public void PreprocessingLexer_DirectiveNotOnNewLine()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Область reg
            F; #КонецОбласти
            ";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => { while (pp.NextLexem().Token != Token.EndOfText) ; });
        }

        [Fact]
        public void PreprocessingLexer_DirectiveNotOnSingleLine()
        {
            var pp = new PreprocessingLexer();

            var code = @"
            #Если Нет
            Тогда
            F;
            #КонецОбласти
            ";

            pp.Code = code;
            Assert.Throws<SyntaxErrorException>(() => { while (pp.NextLexem().Token != Token.EndOfText) ; });
        }

        [Fact]
        public void ConditionalsCompiler_DirectiveNotOnSingleLine()
        {
            var code = @"
            #Если Нет
            Тогда
            F;
            #КонецОбласти
            ";

            var sinkMock = new Mock<IErrorSink>();
            var handler = new ConditionalDirectiveHandler(sinkMock.Object);
            
            var lexer = GetLexerForCode(code);
            var lexem = lexer.NextLexem();

            handler.HandleDirective(ref lexem, lexer);

            sinkMock.Verify(sink => sink.AddError(It.IsAny<ParseError>()), Times.Exactly(2));
        }

        [Fact]
        public void PreprocessingLexer_ExcludedLines()
        {
            var pp = new PreprocessingLexer();
            pp.Define("Да");

            var code = @"
            #Если Да Тогда
            F;
            #Иначе
            !!
            #КонецЕсли
            ";

            pp.Code = code;

            Lexem lex;
            do { lex = pp.NextLexem(); } while (pp.NextLexem().Token != Token.EndOfText);
            Assert.Equal(Token.EndOfText, lex.Token);
        }

        [Fact]
        public void ConditionalsCompiler_ExcludedLines()
        {
            var code = @"
            #Если Да Тогда
            F;
            #Иначе
            !!
            #КонецЕсли
            ";

            var handler = new ConditionalDirectiveHandler(Mock.Of<IErrorSink>());
            handler.Define("Да");
            
            var lexer = GetLexerForCode(code);
            var lexem = lexer.NextLexem();

            while (lexem.Token != Token.EndOfText)
            {
                handler.HandleDirective(ref lexem, lexer);
                while (lexem.Type != LexemType.PreprocessorDirective && lexem.Type != LexemType.EndOfText)
                    lexem = lexer.NextLexem();
            }
            
            Assert.Equal(Token.EndOfText, lexem.Token);
        }

        private string GetPreprocessedContent(ILexer pp, string code)
        {
            pp.Iterator = new SourceCodeIterator(code);
            Lexem lex = Lexem.Empty();

            StringBuilder builder = new StringBuilder();

            while (lex.Type != LexemType.EndOfText)
            {

                lex = pp.NextLexem();

                builder.Append(lex.Content);

            }
            return builder.ToString().Trim();
        }

        private ILexer GetLexerForCode(string code)
        {
            return new FullSourceLexer
            {
                Iterator = new SourceCodeIterator(code)
            };
        }

        private class PreprocessingLexer : ILexer
        {
            private ConditionalDirectiveHandler _handler = new ConditionalDirectiveHandler(new ThrowingErrorSink());
            private ILexer _lexer = new DefaultLexer();

            public Lexem NextLexem()
            {
                var lex = _lexer.NextLexem();
                
                if(lex.Type == LexemType.PreprocessorDirective)
                    _handler.HandleDirective(ref lex, _lexer);

                return lex;
            }

            public SourceCodeIterator Iterator
            {
                get => _lexer.Iterator;
                set => _lexer.Iterator = value;
            }

            public void Define(string item)
            {
                _handler.Define(item);
            }
            
            public void Undef(string item)
            {
                _handler.Undef(item);
            }

            public string Code
            {
                set => Iterator = new SourceCodeIterator(value);
            }
        }
    }
}