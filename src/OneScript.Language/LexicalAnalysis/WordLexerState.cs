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
            bool isEndOfText = false;
            char cs = '\0';
            int start = iterator.Position;
            int currentLine = iterator.CurrentLine;
            int currentColumn = iterator.CurrentColumn;
            while (true)
            {
                if (!isEndOfText)
                {
                    cs = iterator.CurrentSymbol;
                }
                
                if (SpecialChars.IsDelimiter(cs) || isEndOfText)
                {
                    var content = iterator.GetContents();

                    Lexem lex;

                    if (LanguageDef.IsLogicalOperatorString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.Operator,
                            Token = LanguageDef.GetToken(content),
                            Content = content,
                            Location = new CodeRange(start, content.Length, currentLine, currentColumn)
                        };
                    }
                    else if (LanguageDef.IsBooleanLiteralString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.BooleanLiteral,
                            Content = content,
                            Location = new CodeRange(start, content.Length, currentLine, currentColumn)
                        };
                    }
                    else if (LanguageDef.IsUndefinedString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.UndefinedLiteral,
                            Content = content,
                            Location = new CodeRange(start, content.Length, currentLine, currentColumn)
                        };

                    }
                    else if (LanguageDef.IsNullString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.NullLiteral,
                            Content = content,
                            Location = new CodeRange(start, content.Length, currentLine, currentColumn)
                        };

                    }
                    else
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.Identifier,
                            Content = content,
                            Token = LanguageDef.GetToken(content),
                            Location = new CodeRange(start, content.Length, currentLine, currentColumn)
                        };

                        if (LanguageDef.IsBuiltInFunction(lex.Token))
                        {
                            iterator.SkipSpaces();
                            if (iterator.CurrentSymbol != '(')
                            {
                                lex.Token = Token.NotAToken;
                            }
                        }
                    }

                    return lex;
                }

                if (!iterator.MoveNext())
                {
                    isEndOfText = true;
                }
            }
        }
    }
}
