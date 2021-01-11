/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using Xunit;
using Xunit.Sdk;

namespace OneScript.Language.Tests
{
    public class ErrorRecoveryTests
    {
        [Fact]
        public void Successfully_Goes_To_Next_Line()
        {
            var code = "line1 %^ & # (#\n" +
                       "line2 some other stuff\n" +
                       "(*& line3";

            var testLexer = BuildLexer();
            
            testLexer.Iterator = new SourceCodeIterator(code);
            var strategy = new NextLineRecoveryStrategy();
            Lexem lex;

            lex = testLexer.NextLexem();
            lex.Content.Should().Be("line1");
            lex = strategy.Recover(testLexer);
            lex.Content.Should().Be("line2");

            try
            {
                strategy.Recover(testLexer);
            }
            catch (SyntaxErrorException)
            {
                return;
            }
            
            throw new XunitException("should throw unknown symbol");
        }

        [Fact]
        public void NextStatement_Strategy_Rewinds_Through_Errors()
        {
            var code = "ths is a line with error\n" +
                       "#$e d0-f %^ Если\n" +
                       "!@-2 Пока\n" +
                       "!@-2 Перем\n" +
                       "!@-2 Для\n" +
                       "!@-2 Попытка\n";

            var testLexer = BuildLexer();
            
            testLexer.Iterator = new SourceCodeIterator(code);
            var strategy = new NextStatementRecoveryStrategy();
            Lexem lex;

            testLexer.Iterator.MoveToContent();
            
            lex = strategy.Recover(testLexer);
            lex.Token.Should().Be(Token.If);
            
            testLexer.Iterator.MoveNext();
            lex = strategy.Recover(testLexer);
            lex.Token.Should().Be(Token.While);
            
            testLexer.Iterator.MoveNext();
            lex = strategy.Recover(testLexer);
            lex.Token.Should().Be(Token.VarDef);
            
            testLexer.Iterator.MoveNext();
            lex = strategy.Recover(testLexer);
            lex.Token.Should().Be(Token.For);
            
            testLexer.Iterator.MoveNext();
            lex = strategy.Recover(testLexer);
            lex.Token.Should().Be(Token.Try);
        }

        private ILexer BuildLexer()
        {
            var lb = new LexerBuilder();
            lb.DetectWords();
            return lb.Build();
        }
    }
}