/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Language.LexicalAnalysis
{
    public class PreprocessingLexer : ILexemGenerator
    {
        HashSet<string> _definitions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        ILexemGenerator _lexer;
        string _code;

        Lexem _lastExtractedLexem;

        Stack<PreprocessorBlock> _blocks = new Stack<PreprocessorBlock>();
        private int _regionsNesting = 0;

        private class PreprocessorBlock
        {
            public bool IsSolved;
        }

        public PreprocessingLexer()
        {
            _lexer = new Lexer();
        }

        public event EventHandler<PreprocessorUnknownTokenEventArgs> UnknownDirective;

        public void Define(string param)
        {
            _definitions.Add(param);
        }

        public bool IsDefined(string param)
        {
            return _definitions.Contains(param);
        }

        public void Undef(string param)
        {
            _definitions.Remove(param);
        }

        private void PushBlock()
        {
            var block = new PreprocessorBlock()
            {
                IsSolved = false
            };
            _blocks.Push(block);
        }

        private bool BlockIsSolved()
        {
            if (_blocks.Count == 0)
                throw PreprocessorError("Ожидается директива #Если");

            return _blocks.Peek().IsSolved;
        }

        private void MarkAsSolved()
        {
            _blocks.Peek().IsSolved = true;
        }

        private void PopBlock()
        {
            if (_blocks.Count > 0)
                _blocks.Pop();
            else
                throw PreprocessorError("Пропущена директива #Если");
        }

        private int BlockLevel()
        {
            return _blocks.Count;
        }

        private void HandleUnknownDirective(Lexem lex)
        {
            if (UnknownDirective != null)
            {
                var args = new PreprocessorUnknownTokenEventArgs()
                {
                    Lexer = _lexer,
                    Lexem = lex
                };

                UnknownDirective(this, args);
                if (args.IsHandled)
                {
                    _lastExtractedLexem = args.Lexem;
                    return;
                }

            }

            throw PreprocessorError("Неизвестная директива: " + lex.Content);

        }

        private SyntaxErrorException PreprocessorError(string message)
        {
            return new SyntaxErrorException(_lexer.GetCodePosition(), message);
        }

        private bool SolveExpression()
        {
            NextLexem();

            return SolveOrExpression();
        }

        private bool SolveOrExpression()
        {
            var argument = SolveAndExpression();
            if (_lastExtractedLexem.Token == Token.Then)
            {
                return argument;
            }

            if (_lastExtractedLexem.Token == Token.Or)
            {
                NextLexem();
                var secondArgument = SolveOrExpression();
                return argument || secondArgument; // здесь нужны НЕ-сокращенные вычисления
            }

            return argument;
        }

        private bool SolveAndExpression()
        {
            var argument = SolveNotExpression();

            if (_lastExtractedLexem.Token == Token.And)
            {
                NextLexem();
                var secondArgument = SolveAndExpression();
                return argument && secondArgument; // здесь нужны НЕ-сокращенные вычисления
            }

            return argument;
        }

        private bool SolveNotExpression()
        {
            if (_lastExtractedLexem.Token == Token.Not)
            {
                NextLexem();
                return !GetArgument();
            }

            return GetArgument();
        }

        private bool GetArgument()
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                NextLexem();
                var result = SolveOrExpression();
                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    NextLexem();
                    return result;
                }
                throw PreprocessorError("Ожидается закрывающая скобка");
            }

            if (!LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
                throw PreprocessorError("Ожидается объявление препроцессора");
            
            var expression = IsDefined(_lastExtractedLexem.Content);
            NextLexem();
            return expression;
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
                _lexer.Code = _code;
            }
        }

        public int CurrentColumn => _lexer.CurrentColumn;
        public int CurrentLine => _lexer.CurrentLine;
        public CodePositionInfo GetCodePosition() => _lexer.GetCodePosition();
        public SourceCodeIterator Iterator => _lexer.Iterator;

        public Lexem NextLexem()
        {
            MoveNext();

            if (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
                return Preprocess(_lastExtractedLexem);

            if (_lastExtractedLexem.Type == LexemType.EndOfText)
            {
                if (BlockLevel() != 0)
                    throw PreprocessorError("Ожидается завершение директивы препроцессора #Если");
                if (_regionsNesting != 0)
                    throw PreprocessorError("Ожидается завершение директивы препроцессора #Область");
            }

            return _lastExtractedLexem;
        }

        private void MoveNext()
        {
            _lastExtractedLexem = _lexer.NextLexem();
        }

        private Lexem Preprocess(Lexem directive)
        {
            if(directive.Token == Token.If)
            {
                PushBlock();
                return ResolveCondition();
            }
            else if(directive.Token == Token.ElseIf)
            {
                if (BlockIsSolved())
                {
                    SolveExpression(); // проверить корректность условия
                    SkipTillNextDirective();
                    return Preprocess(_lastExtractedLexem);
                }

                return ResolveCondition();
            }
            else if(directive.Token == Token.Else)
            {
                if (BlockIsSolved())
                {
                    SkipTillNextDirective();
                    if (_lastExtractedLexem.Token != Token.EndIf)
                        throw PreprocessorError("Ожидается директива препроцессора #КонецЕсли");

                    return Preprocess(_lastExtractedLexem);
                }

                return LexemFromNewLine();
            }
            else if(directive.Token == Token.EndIf)
            {
                PopBlock();
                return LexemFromNewLine();
            }
            else
            {
                if (LanguageDef.IsPreprocRegion(directive.Content))
                {
                    MoveNext();
                    if (_lexer.Iterator.OnNewLine)
                        throw PreprocessorError("Ожидается имя области");

                    if (!LanguageDef.IsIdentifier(ref _lastExtractedLexem))
                        throw PreprocessorError($"Недопустимое имя Области: {_lastExtractedLexem.Content}");

                    _regionsNesting++;

                    return LexemFromNewLine();
                }
                else if (LanguageDef.IsPreprocEndRegion(directive.Content))
                {
                    if (_regionsNesting == 0)
                        throw PreprocessorError("Пропущена директива препроцессора #Область");

                    _regionsNesting--;

                    return LexemFromNewLine();
                }
            }

            HandleUnknownDirective(_lastExtractedLexem);

            return _lastExtractedLexem;
        }

        private Lexem ResolveCondition()
        {
            var enterBlock = SolveExpression();
            if (_lastExtractedLexem.Token != Token.Then)
                throw PreprocessorError("Ошибка в директиве препроцессора: ожидается ключевое слово Тогда");

            if (enterBlock)
            {
                MarkAsSolved();
                return LexemFromNewLine();
            }
            else
            {
                SkipTillNextDirective();
                return Preprocess(_lastExtractedLexem);
            }
        }

        private Lexem LexemFromNewLine()
        {
            var lex = NextLexem();
            CheckNewLine();

            return lex;
        }

        private void CheckNewLine()
        {
            if (!_lexer.Iterator.OnNewLine)
                throw PreprocessorError("Недопустимые символы в директиве");
        }

        private void SkipTillNextDirective()
        {
            int currentLevel = BlockLevel();
            MoveNext();
            CheckNewLine();

            while (true)
            {
                while (_lastExtractedLexem.Type != LexemType.PreprocessorDirective)
                {
                    if (_lastExtractedLexem.Token == Token.EndOfText)
                        throw PreprocessorError("Ожидается директива препроцессора #КонецЕсли");

                    MoveNext();
                }

                if (_lastExtractedLexem.Token == Token.If)
                    PushBlock();
                else if (_lastExtractedLexem.Token == Token.EndIf && BlockLevel() > currentLevel)
                    PopBlock();
                else if (BlockLevel() == currentLevel)
                    break;

                MoveNext();
            }
        }
    }
}