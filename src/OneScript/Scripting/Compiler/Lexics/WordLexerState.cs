using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler.Lexics
{
    public class WordLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            bool isEndOfText = false;
            Char cs = '\0';
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

                    if (String.Compare(content, "и", true) == 0
                            || String.Compare(content, "или", true) == 0
                            || String.Compare(content, "не", true) == 0
                            || String.Compare(content, "and", true) == 0
                            || String.Compare(content, "or", true) == 0
                            || String.Compare(content, "not", true) == 0)
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.Operator,
                            Token = LanguageDef.GetToken(content),
                            Content = content
                        };
                    }
                    else if (LanguageDef.IsBooleanTrueString(content) 
                        || LanguageDef.IsBooleanFalseString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.BooleanLiteral,
                            Content = content
                        };
                    }
                    else if (LanguageDef.IsUndefinedString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.UndefinedLiteral,
                            Content = content
                        };

                    }
                    else if (LanguageDef.IsNullString(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.NullLiteral,
                            Content = content
                        };

                    }
                    else
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.Identifier,
                            Content = content,
                            Token = LanguageDef.GetToken(content)
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
                    if (isEndOfText)
                    {
                        break;
                    }
                    else
                    {
                        isEndOfText = true;
                    }
                }
            }

            return Lexem.Empty();
        }
    }
}
