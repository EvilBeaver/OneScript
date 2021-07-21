/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ConditionalDirectiveHandler : DirectiveHandlerBase, IDirectiveHandler
    {
        private readonly HashSet<string> _definitions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Stack<PreprocessorBlock> _blocks = new Stack<PreprocessorBlock>();

        private ILexer _lexer;
        Lexem _lastExtractedLexem;

        public ConditionalDirectiveHandler(IErrorSink errorSink) : base(errorSink)
        {
            _lexer = new FullSourceLexer();
        }
        
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
        
        public override void OnModuleEnter()
        {
            base.OnModuleEnter();
            _blocks.Clear();
        }

        public override void OnModuleLeave()
        {
            base.OnModuleLeave();
            if (BlockLevel != 0)
                AddError(LocalizedErrors.EndOfDirectiveExpected("Если")); // FIXME: назвать на том языке, на котором началась директива
        }

        public override bool HandleDirective(ref Lexem lexem, ILexer lexer)
        {
            if (!IsConditional(lexem))
                return default;

            try
            {
                _lexer.Iterator = lexer.Iterator;
                _lastExtractedLexem = lexem;
            
                lexem = Preprocess(_lastExtractedLexem);
            }
            catch (SyntaxErrorException)
            {
                _blocks.Clear();
                throw;
            }
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
                    {
                        return LexemFromNewLine();
                    }

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
            {
                AddError(LocalizedErrors.TokenExpected(Token.Then));
                return LexemFromNewLine();
            }

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
            MoveNextSameLine();

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
                MoveNextSameLine();
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
                MoveNextSameLine();
                var secondArgument = SolveAndExpression();
                return argument && secondArgument; // здесь нужны НЕ-сокращенные вычисления
            }

            return argument;
        }

        private bool SolveNotExpression()
        {
            if (_lastExtractedLexem.Token == Token.Not)
            {
                MoveNextSameLine();
                return !GetArgument();
            }

            return GetArgument();
        }

        private bool GetArgument()
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                MoveNextSameLine();
                var result = SolveOrExpression();
                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    MoveNextSameLine();
                    return result;
                }
                
                AddError(LocalizedErrors.TokenExpected(Token.OpenPar));
                return true; // если ошибка и не было исключения от ErrorSink то войдем в блок
            }

            if (!LanguageDef.IsUserSymbol(_lastExtractedLexem))
            {
                AddError(LocalizedErrors.PreprocessorDefinitionExpected());
                return true;
            }
            
            var expression = IsDefined(_lastExtractedLexem.Content);
            MoveNextSameLine();
            return expression;
        }
        
        private void NextLexem()
        {
            do
            {
                MoveNext();
                if (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
                    _lastExtractedLexem = Preprocess(_lastExtractedLexem);
            }
            while (_lastExtractedLexem.Type == LexemType.Comment);

            switch (_lastExtractedLexem.Type)
            {
                case LexemType.PreprocessorDirective:
                    Preprocess(_lastExtractedLexem);
                    break;
                case LexemType.EndOfText when BlockLevel != 0:
                    AddError(LocalizedErrors.EndOfDirectiveExpected("Если"));
                    break;
            }
        }
        
        private void MoveNextSameLine()
        {
            _lastExtractedLexem = _lexer.NextLexem();
            if (_lexer.Iterator.OnNewLine)
            {
                AddError("Неожиданное завершение директивы");
                var recovery = new NextLineRecoveryStrategy();
                recovery.Recover(_lexer);
            }
        }
        
        private int BlockLevel => _blocks.Count;

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
            {
                AddError(LocalizedErrors.DirectiveExpected("Если"));
                return true; // зайдем внутрь для синтаксического контроля
            }

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
                AddError(LocalizedErrors.DirectiveIsMissing("Если"));
        }

        private void AddError(string message)
        {
            var err = new ParseError
            {
                Description = message,
                Position = _lexer.GetErrorPosition(),
                ErrorId = "PreprocessorError"
            };
            
            ErrorSink.AddError(err);
        }
        
        private void AddError(ParseError err)
        {
            err.Position = _lexer.GetErrorPosition();
            ErrorSink.AddError(err);
        }

        private void MoveNext()
        {
            _lastExtractedLexem = _lexer.NextLexem();
        }
        
        private void CheckNewLine()
        {
            if (!_lexer.Iterator.OnNewLine)
            {
                AddError("Недопустимые символы в директиве");
                _lexer.ReadToLineEnd();
            }
        }

        private void SkipTillNextDirective()
        {
            int currentLevel = BlockLevel;
            var lineTail = _lexer.Iterator.ReadToLineEnd();
            if(lineTail.Length > 0 && !lineTail.StartsWith("//"))
            {
                AddError("Недопустимые символы в директиве");
                _lexer.ReadToLineEnd();
            }

            while (true)
            {
                if (!FindHashSign())
                {
                    AddError(LocalizedErrors.DirectiveExpected("КонецЕсли"));
                    return;
                }
                MoveNext();
                
                if (_lastExtractedLexem.Token == Token.If)
                    PushBlock();
                else if (_lastExtractedLexem.Token == Token.EndIf && BlockLevel > currentLevel)
                    PopBlock();
                else if (BlockLevel == currentLevel &&
                         (_lastExtractedLexem.Token == Token.EndIf || 
                          _lastExtractedLexem.Token == Token.ElseIf ||
                          _lastExtractedLexem.Token == Token.Else) )
                    break;
            }
        }

        private bool FindHashSign()
        {
            var iterator = _lexer.Iterator;

            while (true)
            {
                if (iterator.CurrentSymbol == SpecialChars.Preprocessor)
                {
                    if(iterator.OnNewLine)
                        return true;
                }

                if (!iterator.MoveNext())
                    break;

                iterator.SkipSpaces();
            }

            return false;
        }

        private Lexem LexemFromNewLine()
        {
            NextLexem();
            CheckNewLine();

            return _lastExtractedLexem;
        }
    }
}