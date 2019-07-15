using System.Text;
using OneScript.Language.LexicalAnalysis;
using Xunit;

namespace OneScript.Language.Tests
{
   public class PreprocessorTests
    {
        [Fact]
        public void Definitions_Are_Defined()
        {
            var pp = new PreprocessingLexer();;
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

        private string GetPreprocessedContent(PreprocessingLexer pp, string code)
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