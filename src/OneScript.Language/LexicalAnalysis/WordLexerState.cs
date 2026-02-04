/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public class WordLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            var location = new CodeRange(iterator.CurrentLine, iterator.CurrentColumn);

            do
            {
                if (SpecialChars.IsDelimiter(iterator.CurrentSymbol))
                    break;
            }
            while (iterator.MoveNext());

            var content = iterator.GetContents();
            Lexem lex;

            switch (LanguageDef.GetWordType(content))
            {
                case LanguageDef.WordType.Logical:
                    lex = new Lexem()
                    {
                        Type = LexemType.Operator,
                        Token = LanguageDef.GetToken(content),
                        Content = content,
                        Location = location
                    };
                    break;

                case LanguageDef.WordType.Boolean:
                    lex = new Lexem()
                    {
                        Type = LexemType.BooleanLiteral,
                        Content = content,
                        Location = location
                    };
                    break;
                
                case LanguageDef.WordType.Undefined:
                    lex = new Lexem()
                    {
                        Type = LexemType.UndefinedLiteral,
                        Content = content,
                        Location = location
                    };
                    break;

                case LanguageDef.WordType.Null:
                    lex = new Lexem()
                    {
                        Type = LexemType.NullLiteral,
                        Content = content,
                        Location = location
                    };
                    break;

                default:
                    var tok = LanguageDef.GetToken(content);
                    if (LanguageDef.IsBuiltInFunction(tok))
                    {
                        if (iterator.ReadNextChar() != '(')
                        {
                            tok = Token.NotAToken;
                        }
                    }

                    lex = new Lexem()
                    {
                        Type = LexemType.Identifier,
                        Content = content,
                        Token = tok,
                        Location = location
                    };
                    break;
            }

            return lex;
        }
    }
}
