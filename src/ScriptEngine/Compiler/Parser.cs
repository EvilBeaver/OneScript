using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Compiler
{
    class Parser
    {
        private string _code;
        private ParserState _state;
        private ParseIterator _iterator;

        ParserState _emptyState = new EmptyParserState();
        ParserState _wordState = new WordParserState();
        ParserState _numberState = new NumberParserState();
        ParserState _stringState = new StringParserState();
        ParserState _operatorState = new OperatorParserState();
        ParserState _dateState = new DateParserState();

        public string Code 
        { 
            get { return _code; } 
            set { _code = value; } 
        }

        public void Start()
        {
            _iterator = new ParseIterator(_code);
        }

        public int CurrentLine
        {
            get
            {
                return _iterator.CurrentLine;
            }
        }

        public string GetCodeLine(int index)
        {
            int start = _iterator.GetLineBound(index);
            int end = _code.IndexOf('\n', start);
            if (end >= 0)
            {
                return _code.Substring(start, end - start);
            }
            else
            {
                return _code.Substring(start);
            }
        }

        public Lexem NextLexem()
        {
            _state = _emptyState;

            while (true)
            {
                if (_iterator.MoveToContent())
                {
                    char cs = _iterator.CurrentSymbol;
                    if (Char.IsLetter(cs) || cs == '_')
                    {
                        _state = _wordState;
                    }
                    else if (Char.IsDigit(cs))
                    {
                        _state = _numberState;
                    }
                    else if (cs == SpecialChars.DateQuote)
                    {
                        _state = _dateState;
                    }
                    else if (cs == SpecialChars.StringQuote)
                    {
                        _state = _stringState;
                    }
                    else if (SpecialChars.IsOperatorChar(cs))
                    {
                        _state = _operatorState;
                    }
                    else if (cs == SpecialChars.EndOperator)
                    {
                        _iterator.MoveNext();
                        return new Lexem() { Type = LexemType.EndOperator, Token = Token.Semicolon };
                    }
                    else if (cs == '?')
                    {
                        _iterator.MoveNext();
                        return new Lexem() { Type = LexemType.Identifier, Token = Token.Question };
                    }
                    else
                    {
                        throw new ParserException(string.Format("Unknown character {0}", cs), _iterator.CurrentLine);
                    }

                    var lex = _state.ReadNextLexem(_iterator);
                    if (lex.Type == LexemType.NotALexem)
                    {
                        _state = _emptyState;
                        continue;
                    }

                    return lex;

                }
                else
                {
                    return Lexem.EndOfText();
                }
            }

        }

    }

    struct Word
    {
        public int start;
        public string content;
    }

	struct Lexem
    {
        public LexemType Type;
        public string Content;
        public Token Token;

        public static Lexem Empty()
        {
            return new Lexem() { Type = LexemType.NotALexem };
        }

        public static Lexem EndOfText()
        {
            return new Lexem() { Type = LexemType.EndOfText, Token = Token.EndOfText };
        }
        
        public override string ToString()
        {
            return string.Format("{0}:{1}", Enum.GetName(typeof(LexemType), this.Type), Content);
        }

    }
	
    enum LexemType
    {
        NotALexem,
        Identifier,
        Operator,
        StringLiteral,
        DateLiteral,
        NumberLiteral,
        BooleanLiteral,
        UndefinedLiteral,
        EndOperator,
        EndOfText
    }

    abstract class ParserState
    {
        abstract public Lexem ReadNextLexem(ParseIterator iterator);
    }

    class EmptyParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            return Lexem.Empty();
        }
    }

    class WordParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
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
                    var content = iterator.GetContents().content;
                    
                    Lexem lex;

                    if(String.Compare(content, "и", true) == 0
                            || String.Compare(content, "или", true) == 0
                            || String.Compare(content, "не", true) == 0)
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.Operator,
                            Token = LanguageDef.GetToken(content),
                            Content = content
                        };
                    }
                    else if (String.Compare(content, "истина", true) == 0
                        || String.Compare(content, "ложь", true) == 0)
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.BooleanLiteral,
                            Content = content
                        };
                    }
                    else if (String.Compare(content, "неопределено", true) == 0)
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.UndefinedLiteral,
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

    class StringParserState : ParserState
    {

        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            bool hasEscapedQuotes = false;

            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;
                if (cs == SpecialChars.StringQuote)
                {
                    // либо конец литерала, либо escape кавычки
                    if (iterator.MoveNext())
                    {
                        if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                        {
                            hasEscapedQuotes = true;
                        }
                        else
                        {
                            iterator.MoveBack();
                            var lex = new Lexem()
                            {
                                Type = LexemType.StringLiteral,
                                Content = iterator.GetContents(1, 1).content
                            };

                            if (hasEscapedQuotes)
                            {
                                lex.Content = lex.Content.Replace("\"\"", "\"");
                            }

                            var newLinePosition = lex.Content.IndexOf('\n');
                            while (newLinePosition >= 0)
                            {
                                var paddingPosition = lex.Content.IndexOf('|', newLinePosition);
                                if (paddingPosition < 0)
                                {
                                    throw new ParserException("Unclosed string literal", iterator.CurrentLine);
                                }

                                string head;
                                string tail;

                                head = lex.Content.Substring(0, newLinePosition + 1); // including \n
                                if (paddingPosition == lex.Content.Length - 1)
                                    tail = "";
                                else
                                    tail = lex.Content.Substring(paddingPosition + 1);

                                lex.Content = head + tail;
                                newLinePosition = lex.Content.IndexOf('\n', newLinePosition + 1);
                            }

                            iterator.MoveNext();

                            return lex;
                        }
                    }
                }
                else if (cs == '\n')
                {
                    iterator.MoveNext();
                    iterator.SkipSpaces();
                    if (iterator.CurrentSymbol != '|')
                    {
                        throw new ParserException("Wrong string literal", iterator.CurrentLine);
                    }
                }
            }

            throw new ParserException("Unclosed string literal", iterator.CurrentLine);
        }
    }

    class DateParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;
                if (cs == SpecialChars.DateQuote)
                {
                    var lex = new Lexem()
                    {
                        Type = LexemType.DateLiteral,
                        Content = iterator.GetContents(1, 1).content
                    };

                    iterator.MoveNext();

                    return lex;
                }
                else if(!Char.IsDigit(cs))
                {
                    throw new ParserException("Incorrect date literal", iterator.CurrentLine);
                }
            }

            throw new ParserException("Unclosed date literal", iterator.CurrentLine);
        }
    }

    class OperatorParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            if(iterator.CurrentSymbol == '<')
            {
                if (iterator.MoveNext())
                {
                    var next = iterator.CurrentSymbol;
                    if (next != '>' && next != '=')
                    {
                        iterator.MoveBack();
                    }
                    else
                    {
                        iterator.MoveNext();
                        return ExtractOperatorContent(iterator);
                    }
                }
            }
            else if(iterator.CurrentSymbol == '>')
            {
                if (iterator.MoveNext())
                {
                    var next = iterator.CurrentSymbol;
                    if (next != '=')
                    {
                        iterator.MoveBack();
                    }
                    else
                    {
                        iterator.MoveNext();
                        return ExtractOperatorContent(iterator);
                    }
                }
            }
            else if (iterator.CurrentSymbol == '/')
            {
                if (iterator.MoveNext())
                {
                    if (iterator.CurrentSymbol == '/')
                    {
                        // это комментарий
                        while (iterator.MoveNext())
                        {
                            if (iterator.CurrentSymbol == '\n')
                            {
                                iterator.GetContents();
                                return Lexem.Empty();
                            }
                        }
                        iterator.GetContents();
                        return Lexem.EndOfText();
                    }
                    else
                    {
                        iterator.MoveBack();
                    }
                }
            }

            var lex = ExtractOperatorContent(iterator);
            
            iterator.MoveNext();

            return lex;
            
        }

        private static Lexem ExtractOperatorContent(ParseIterator iterator)
        {
            Lexem lex;
            var content = iterator.GetContents().content;
            lex = new Lexem()
            {
                Type = LexemType.Operator,
                Content = content,
                Token = LanguageDef.GetToken(content)
            };
            return lex;
        }
    }

    class NumberParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            bool hasDecimalPoint = false;
            while (true)
            {
                if (Char.IsDigit(iterator.CurrentSymbol))
                {
                    if (!iterator.MoveNext())
                    {
                        var lex = new Lexem()
                        {
                            Type = LexemType.NumberLiteral,
                            Content = iterator.GetContents().content
                        };

                        return lex;
                    }
                }
                else if (SpecialChars.IsDelimiter(iterator.CurrentSymbol))
                {
                    if (iterator.CurrentSymbol == '.')
                    {
                        if (!hasDecimalPoint)
                        {
                            hasDecimalPoint = true;
                            iterator.MoveNext();
                            continue;
                        }
                        else
                        {
                            throw new ParserException("Syntax error: decimal point", iterator.CurrentLine);
                        }
                    }

                    var lex = new Lexem()
                    {
                        Type = LexemType.NumberLiteral,
                        Content = iterator.GetContents().content
                    };

                    return lex;
                }
                else
                {
                    throw new ParserException("Unexpected character", iterator.CurrentLine);
                }
            }
        }
    }

    public class ParserException : ApplicationException
    {
        int _line;

        public int Line
        {
            get { return _line; }
        }

        public ParserException(string msg, int line) : base(msg)
        {
            _line = line;
        }

        public override string ToString()
        {
            return base.ToString() + "\nLine: " + _line;
        }
    }
}
