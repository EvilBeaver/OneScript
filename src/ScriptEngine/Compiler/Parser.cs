/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Compiler
{
    class Parser
    {
        private string _code;
        private ParseIterator _iterator;

        private readonly ParserState _emptyState = new EmptyParserState();
        private readonly ParserState _wordState = new WordParserState();
        private readonly ParserState _numberState = new NumberParserState();
        private readonly ParserState _stringState = new StringParserState();
        private readonly ParserState _operatorState = new OperatorParserState();
        private readonly ParserState _dateState = new DateParserState();
        private readonly ParserState _directiveState = new DirectiveParserState();

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
            return _iterator.GetCodeLine(index);
        }

        public Lexem NextLexem()
        {
            while (true)
            {
                if (_iterator.MoveToContent())
                {
                    ParserState state;

                    char cs = _iterator.CurrentSymbol;
                    if (Char.IsLetter(cs) || cs == '_')
                    {
                        state = _wordState;
                    }
                    else if (Char.IsDigit(cs))
                    {
                        state = _numberState;
                    }
                    else if (cs == SpecialChars.DateQuote)
                    {
                        state = _dateState;
                    }
                    else if (cs == SpecialChars.StringQuote)
                    {
                        state = _stringState;
                    }
                    else if (SpecialChars.IsOperatorChar(cs))
                    {
                        state = _operatorState;
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
                    else if(cs == SpecialChars.Directive)
                    {
                        state = _directiveState;
                    }
                    else
                    {
                        var cp = _iterator.GetPositionInfo(_iterator.CurrentLine);
                        throw new ParserException(cp, string.Format("Неизвестный символ {0}", cs));
                    }

                    var lex = state.ReadNextLexem(_iterator);
                    if (lex.Type == LexemType.NotALexem)
                    {
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


        internal SourceCodeIndexer GetCodeIndexer()
        {
            return _iterator.GetCodeIndexer();
        }

        internal string ReadLineToEnd()
        {
            while (_iterator.CurrentSymbol != '\n')
            {
                if (!_iterator.MoveNext ())
                    break;
            }
            string content = _iterator.GetContents().content;
            return content == null ? "" : content;
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
        NullLiteral,
        EndOperator,
        EndOfText,
        Directive
    }

    abstract class ParserState
    {
        abstract public Lexem ReadNextLexem(ParseIterator iterator);
        public ParserException CreateExceptionOnCurrentLine(string message, ParseIterator iterator)
        {
            var cp = iterator.GetPositionInfo(iterator.CurrentLine);
            return new ParserException(cp, message);
        }
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
        readonly HashSet<string> _booleanOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        readonly HashSet<string> _booleanLiterals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        readonly HashSet<string> _undefined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public WordParserState()
        {
            _booleanOperators.Add("и");
            _booleanOperators.Add("или");
            _booleanOperators.Add("не");
            _booleanOperators.Add("and");
            _booleanOperators.Add("or");
            _booleanOperators.Add("not");

            _booleanLiterals.Add("истина");
            _booleanLiterals.Add("ложь");
            _booleanLiterals.Add("true");
            _booleanLiterals.Add("false");

            _undefined.Add("неопределено");
            _undefined.Add("undefined");
                
        }

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

                    if(_booleanOperators.Contains(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.Operator,
                            Token = LanguageDef.GetToken(content),
                            Content = content
                        };
                    }
                    else if (_booleanLiterals.Contains(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.BooleanLiteral,
                            Content = content
                        };
                    }
                    else if (_undefined.Contains(content))
                    {
                        lex = new Lexem()
                        {
                            Type = LexemType.UndefinedLiteral,
                            Content = content
                        };

                    }
                    else if (String.Compare(content, "null", true) == 0)
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

    class StringParserState : ParserState
    {
        private void SkipSpacesAndComments(ParseIterator iterator)
        {
            while (true)
            {   /* Пропускаем все пробелы и комментарии */
                iterator.SkipSpaces();

                if (iterator.CurrentSymbol == '/')
                {
                    if (!iterator.MoveNext())
                        throw CreateExceptionOnCurrentLine("Некорректный символ", iterator);

                    if (iterator.CurrentSymbol != '/')
                        throw CreateExceptionOnCurrentLine("Некорректный символ", iterator);

                    do
                    {
                        if (!iterator.MoveNext())
                            break;

                    } while (iterator.CurrentSymbol != '\n');

                }
                else
                    break;
            }
        }

        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            StringBuilder ContentBuilder = new StringBuilder();

            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;

                if (cs == SpecialChars.StringQuote)
                {
                    iterator.MoveNext();
                    if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                    { /* Двойная кавычка */
                        ContentBuilder.Append("\"");
                        continue;
                    }

                    /* Завершение строки */
                    SkipSpacesAndComments(iterator);

                    if (iterator.CurrentSymbol == SpecialChars.StringQuote)
                    {   /* Сразу же началась новая строка */
                        ContentBuilder.Append('\n');
                        continue;
                    }

                    var lex = new Lexem
                    {
                        Type = LexemType.StringLiteral,
                        Content = ContentBuilder.ToString()
                    };
                    return lex;
                }
                else if(cs == '\r')
                {
                    continue;
                }
                else if (cs == '\n')
                {
                    iterator.MoveNext();
                    SkipSpacesAndComments(iterator);

                    if (iterator.CurrentSymbol != '|')
                        throw CreateExceptionOnCurrentLine("Некорректный строковый литерал!", iterator);

                    ContentBuilder.Append('\n');

                    continue;
                }
                else
                    ContentBuilder.Append(cs);

            }

            throw CreateExceptionOnCurrentLine("Незавершённый строковой интервал!", iterator);

        }
    }

    class DateParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            var numbers = new StringBuilder();

            while (iterator.MoveNext())
            {
                var cs = iterator.CurrentSymbol;
                if (cs == SpecialChars.DateQuote)
                {
                    iterator.GetContents(1, 1);
                    iterator.MoveNext();

                    var lex = new Lexem()
                    {
                        Type = LexemType.DateLiteral,
                        Content = numbers.ToString()
                    };

                    return lex;
                }
                else if(Char.IsDigit(cs))
                {
                    numbers.Append(cs);
                }

                if (numbers.Length > 14)
                    throw CreateExceptionOnCurrentLine("Некорректный литерал даты", iterator);
            }

            throw CreateExceptionOnCurrentLine("Незавершенный литерал даты", iterator);
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
                            throw CreateExceptionOnCurrentLine("Некорректно указана десятичная точка в числе", iterator);
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
                    throw CreateExceptionOnCurrentLine("Некорректный символ", iterator);
                }
            }
        }
    }

    class DirectiveParserState : ParserState
    {
        public override Lexem ReadNextLexem(ParseIterator iterator)
        {
            System.Diagnostics.Debug.Assert(iterator.CurrentSymbol == SpecialChars.Directive);
            iterator.MoveNext();
            if (!iterator.MoveToContent())
                throw CreateExceptionOnCurrentLine("Ожидается директива", iterator);


            var wps = new WordParserState();
            var lex = wps.ReadNextLexem(iterator);
            if (lex.Type == LexemType.Identifier && lex.Token == Token.NotAToken)
            {
                lex.Type = LexemType.Directive;
            }
            else
                throw CreateExceptionOnCurrentLine("Ожидается директива", iterator);

            return lex;
        }

    }

}
