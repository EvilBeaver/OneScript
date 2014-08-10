using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class Lexer
    {
        private string _code;
        private SourceCodeIterator _iterator;
        private LexerState _state;
        
        private LexerState _emptyState = new EmptyLexerState();
        private LexerState _wordState = new WordLexerState();
        private LexerState _numberState = new NumberLexerState();
        private LexerState _stringState = new StringLexerState();
        private LexerState _operatorState = new OperatorLexerState();
        private LexerState _dateState = new DateLexerState();
        private Preprocessor _preprocessor = new Preprocessor();

        public Lexer()
        {
            _iterator = new SourceCodeIterator(null);
        }

        public int CurrentColumn
        {
            get
            {
                return _iterator.CurrentColumn;
            }
        }

        public int CurrentLine
        {
            get
            {
                return _iterator.CurrentLine;
            }
        }
        public string Code 
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                _iterator = new SourceCodeIterator(value);
            }
        }

        public void DefinePreprocessorToken(string token)
        {
            _preprocessor.Define(token);
        }

        public void UndefPreprocessorToken(string token)
        {
            _preprocessor.Undef(token);
        }

        public Lexem NextLexem()
        {
            _state = _emptyState;

            while (true)
            {
                if (_iterator.MoveToContent())
                {
                    if (_iterator.CurrentSymbol != '#')
                    {
                        SelectState();
                    }
                    else
                    {
                        Preprocess();
                    }

                    Lexem lex;
                    try
                    {
                        lex = _state.ReadNextLexem(_iterator);
                        if (lex.Type == LexemType.NotALexem) //комментарии
                        {
                            _state = _emptyState;
                            continue;
                        }
                    }
                    catch (SyntaxErrorException exc)
                    {
                        if (HandleError(exc))
                        {
                            lex = Lexem.Empty();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return lex;

                }
                else
                {
                    return Lexem.EndOfText();
                }
            }
        }

        private void Preprocess()
        {
            while (_iterator.Position != Lexer.OUT_OF_TEXT)
            {
                if (_preprocessor.Solve(_iterator))
                {
                    SelectState();
                    return;
                }
                else
                {
                    while (_iterator.CurrentSymbol != '#')
                    {
                        SelectState();
                        _state.ReadNextLexem(_iterator);
                        if(!_iterator.MoveToContent())
                        {
                            _state = _emptyState;
                        }
                    }
                }
            }
        }

        private LexerState SelectState()
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
                _state = new FixedParserState(new Lexem()
                {
                    Type = LexemType.EndOperator,
                    Token = Token.Semicolon
                });
            }
            else if (cs == '?')
            {
                _iterator.MoveNext();
                _state = new FixedParserState(new Lexem()
                {
                    Type = LexemType.Operator,
                    Token = Token.Question
                });
            }
            else
            {
                var cp = _iterator.GetPositionInfo();
                var exc = new SyntaxErrorException(cp, string.Format("Неизвестный символ {0}", cs));
                if (!HandleError(exc))
                {
                    throw exc;
                }
            }

            return _state;
        }

        public SourceCodeIterator GetIterator()
        {
            return _iterator;
        }

        private bool HandleError(SyntaxErrorException exc)
        {
            if (UnexpectedCharacterFound != null)
            {
                var eventArgs = new LexerErrorEventArgs();
                eventArgs.IsHandled = false;
                eventArgs.Iterator = _iterator;
                eventArgs.CurrentState = _state;
                eventArgs.Exception = exc;

                UnexpectedCharacterFound(this, eventArgs);

                return eventArgs.IsHandled;

            }
            else
            {
                return false;
            }
        }

        public event EventHandler<LexerErrorEventArgs> UnexpectedCharacterFound;

        public const int OUT_OF_TEXT = -1;
    }

    public class LexerErrorEventArgs : EventArgs
    {
        public bool IsHandled { get; set; }
        public SourceCodeIterator Iterator { get; set; }
        public LexerState CurrentState { get; set; }
        public SyntaxErrorException Exception{ get; set; }
    }
}
