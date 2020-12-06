﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.LexicalAnalysis
{
    public class FullSourceLexer : ILexemGenerator
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
        private LexerState _commentState = new CommentLexerState();
        private LexerState _annotationState = new AnnotationLexerState();
        private LexerState _directiveState = new PreprocessorDirectiveLexerState();
        
        private FixedLexerState _fixedState = new FixedLexerState();

        public FullSourceLexer()
        {
            _iterator = new SourceCodeIterator();
        }

        public SourceCodeIterator Iterator
        {
            get => _iterator;
            set => _iterator = value;
        }

        public int CurrentColumn => _iterator.CurrentColumn;

        public int CurrentLine => _iterator.CurrentLine;

        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                _iterator = new SourceCodeIterator(value);
            }
        }

        public virtual Lexem NextLexem()
        {
            _state = _emptyState;

            while (true)
            {
                if (_iterator.MoveToContent())
                {
                    SelectState();

                    Lexem lex;
                    try
                    {
                        lex = _state.ReadNextLexem(_iterator);

                        if (lex.Type == LexemType.NotALexem) // обработанные синтакс-ошибки
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

        private void SelectState()
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
                _state = CommentOrOperatorState(cs);
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
            else if (cs == SpecialChars.Annotation)
            {
                _iterator.GetContents();
                _iterator.MoveNext();
                _state = _annotationState;
            }
            else
            {
                var cp = _iterator.GetErrorPosition();
                var exc = new SyntaxErrorException(cp, string.Format("Неизвестный символ {0}", cs));
                if (!HandleError(exc))
                {
                    throw exc;
                }
            }
        }

        private LexerState CommentOrOperatorState(char cs)
        {
            if (cs == '/' && _iterator.PeekNext() == '/')
            {
                return _commentState;
            }

            return _operatorState;
        }

        private void SetFixedState(LexemType lexemType, Token token)
        {
            _iterator.MoveNext();
            _fixedState.SetOutput(new Lexem()
            {
                Type = lexemType,
                Token = token,
                Location = new CodeRange(CurrentLine, CurrentColumn)
            });

            _state = _fixedState;

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
