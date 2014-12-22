using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler.Lexics
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
        private FixedParserState _fixedState = new FixedParserState();
        private PreprocessorDirectiveLexerState _directiveState = new PreprocessorDirectiveLexerState();

        private class FixedParserState : LexerState
        {
            Lexem _lex;

            public void SetOutput(Lexem lex)
            {
                _lex = lex;
            }

            public override Lexem ReadNextLexem(SourceCodeIterator iterator)
            {
                return _lex;
            }
        }

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
            try
            {
                while (_iterator.Position != Lexer.OUT_OF_TEXT)
                {
                    if (_preprocessor.Solve(_iterator))
                    {
                        if(_iterator.CurrentSymbol == '#')
                        {
                            Preprocess();
                            return;
                        }

                        SelectState();
                        return;
                    }
                    else
                    {
                        while (_iterator.CurrentSymbol != '#')
                        {
                            SelectState();
                            _state.ReadNextLexem(_iterator);
                            if (!_iterator.MoveToContent())
                            {
                                _state = _emptyState;
                            }
                        }
                    }
                }
            }
            catch (SyntaxErrorException exc)
            {
                if(!HandleError(exc))
                    throw;
            }
        }

        private LexerState SelectState()
        {
            char cs = _iterator.CurrentSymbol;
            if (Char.IsLetter(cs) || cs == SpecialChars.Underscore)
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
                SetFixedState(LexemType.EndOperator, Token.Semicolon);
            }
            else if (cs == SpecialChars.QuestionMark)
            {
                SetFixedState(LexemType.Operator, Token.Question);
            }
            else if(cs == SpecialChars.Preprocessor)
            {
                _state = _directiveState;
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

        private void SetFixedState(LexemType lexemType, Token token)
        {
            _iterator.MoveNext();
            _fixedState.SetOutput(new Lexem()
            {
                Type = lexemType,
                Token = token
            });

            _state = _fixedState;

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
        public event EventHandler<PreprocessorUnknownTokenEventArgs> UnknownPreprocessorToken
        {
            add
            {
                _preprocessor.UnknownDirective += value;
            }
            remove
            {
                _preprocessor.UnknownDirective -= value;
            }
        }

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
