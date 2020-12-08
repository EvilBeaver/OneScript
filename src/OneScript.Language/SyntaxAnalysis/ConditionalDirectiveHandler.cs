/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ConditionalDirectiveHandler : IDirectiveHandler
    {
        private readonly HashSet<string> _definitions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Stack<PreprocessorBlock> _blocks = new Stack<PreprocessorBlock>();

        private ILexer _lexer;
        Lexem _lastExtractedLexem;
        
        private class PreprocessorBlock
        {
            public bool IsSolved;
        }
        
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
        
        public void OnModuleEnter(ParserContext context)
        {
        }

        public void OnModuleLeave(ParserContext context)
        {
            if (BlockLevel() != 0)
                throw PreprocessorError(context.Lexer, "Ожидается завершение директивы препроцессора #Если");
        }

        public bool HandleDirective(ParserContext context)
        {
            var lexem = context.LastExtractedLexem;
            if (!IsConditional(lexem))
                return default;
            
            _lexer = context.Lexer;
            _lastExtractedLexem = lexem;
            
            context.LastExtractedLexem = Preprocess(_lastExtractedLexem);
            return true;
        }

        private static bool IsConditional(in Lexem lastExtractedLexem)
        {
            bool isConditional;
            switch (lastExtractedLexem.Token)
            {
                case Token.If:
                case Token.ElseIf:
                case Token.Else:
                case Token.EndIf:
                    isConditional = true;
                    break;
                default:
                    isConditional = false;
                    break;
            }

            return isConditional;
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

            if (!LanguageDef.IsUserSymbol(_lastExtractedLexem))
                throw PreprocessorError("Ожидается объявление препроцессора");
            
            var expression = IsDefined(_lastExtractedLexem.Content);
            NextLexem();
            return expression;
        }
        
        private void NextLexem()
        {
            MoveNext();

            switch (_lastExtractedLexem.Type)
            {
                case LexemType.PreprocessorDirective:
                    Preprocess(_lastExtractedLexem);
                    break;
                case LexemType.EndOfText when BlockLevel() != 0:
                    throw PreprocessorError("Ожидается завершение директивы препроцессора #Если");
            }
        }
        
        private int BlockLevel()
        {
            return _blocks.Count;
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
        
        private static SyntaxErrorException PreprocessorError(ILexer lexer, string message)
        {
            return new SyntaxErrorException(lexer.GetErrorPosition(), message);
        }
        
        private SyntaxErrorException PreprocessorError(string message)
        {
            return PreprocessorError(_lexer, message);
        }
        
        private void MoveNext()
        {
            _lastExtractedLexem = _lexer.NextLexem();
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

                var doBreak = false;
                switch (_lastExtractedLexem.Token)
                {
                    case Token.If:
                        PushBlock();
                        break;
                    case Token.ElseIf:
                    case Token.Else:
                        if (BlockLevel() == currentLevel)
                            doBreak = true;
                        break;
                    case Token.EndIf:
                        if(BlockLevel() > currentLevel)
                            PopBlock();
                        break;
                }
                
                if(doBreak)
                    break;
                
                MoveNext();
            }
        }
        
        private Lexem LexemFromNewLine()
        {
            NextLexem();
            CheckNewLine();

            return _lastExtractedLexem;
        }
    }
}