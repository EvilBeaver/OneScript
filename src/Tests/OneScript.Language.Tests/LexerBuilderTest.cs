/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Localization;
using Xunit;
using Xunit.Sdk;

namespace OneScript.Language.Tests
{
    public class LexerBuilderTest
    {
        [Fact]
        public void Can_Build_SelectorClause()
        {
            var builder = new LexerBuilder();

            builder.Detect((cs,i) => char.IsLetter(cs) || cs == SpecialChars.Underscore)
                .HandleWith(new WordLexerState());
            
            builder.Detect((cs, i) => char.IsDigit(cs))
                .HandleWith(new NumberLexerState());

            var lexer = builder.Build();

            lexer.Iterator = MakeCodeIterator("1 Hello 2 (");

            var lexem1 = lexer.NextLexem();
            var lexem2 = lexer.NextLexem();
            var lexem3 = lexer.NextLexem();

            try
            {
                lexer.NextLexem();
                throw new XunitException("Must throw error");
            }
            catch (SyntaxErrorException e)
            {
                var localeString = BilingualString.Localize("Неизвестный символ", "Unexpected character");
                Assert.Contains(localeString, e.Message);
            }
            
            Assert.Equal(LexemType.NumberLiteral, lexem1.Type);
            Assert.Equal(LexemType.Identifier, lexem2.Type);
            Assert.Equal(LexemType.NumberLiteral, lexem3.Type);
        }

        private SourceCodeIterator MakeCodeIterator(string code)
        {
            return SourceCodeHelper.FromString(code).CreateIterator();
        }
    }
}