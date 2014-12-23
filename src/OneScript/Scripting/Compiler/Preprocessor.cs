using OneScript.Scripting.Compiler.Lexics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler
{
    public class Preprocessor : ILexemGenerator
    {
        HashSet<string> _definitions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        Lexer _lexer;
        string _code;

        Lexem _lastExtractedLexem;

        Stack<PreprocessorBlock> _blocks = new Stack<PreprocessorBlock>();

        private class PreprocessorBlock
        {
            public bool OutputIsOn;
            public bool IsSolved;
        }

        public Preprocessor()
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

        private bool OutputIsOn()
        {
            if (_blocks.Count > 0)
                return _blocks.Peek().OutputIsOn;
            else
                return true;
        }

        private void PushBlock(bool isOn)
        {
            var block = new PreprocessorBlock();
            block.OutputIsOn = isOn;
            if (block.OutputIsOn)
                block.IsSolved = true;
            _blocks.Push(block);
        }

        private bool BlockIsSolved()
        {
            if(_blocks.Count == 0)
                throw PreprocessorError("Ожидается директива #Если");

            return _blocks.Peek().IsSolved;
        }

        private void MarkAsSolved()
        {
            _blocks.Peek().IsSolved = true;
        }

        private void SetOutput(bool output)
        {
            _blocks.Peek().OutputIsOn = output;
        }

        private void PopBlock()
        {
            if(_blocks.Count > 0)
                _blocks.Pop();
        }

        private int BlockLevel()
        {
            return _blocks.Count;
        }

        private void HandleUnknownDirective(Lexem lex)
        {
            if(UnknownDirective != null)
            {
                var args = new PreprocessorUnknownTokenEventArgs()
                {
                    Iterator = _lexer.GetIterator(),
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

        private bool SolveExpression(SourceCodeIterator iterator)
        {
            var priority = new Dictionary<Token, int>();
            priority.Add(Token.Or, 1);
            priority.Add(Token.And, 2);
            priority.Add(Token.Not, 3);

            List<bool?> expressionResult = new List<bool?>();
            List<Token> orderedOperators = new List<Token>();
            Stack<Token> operators = new Stack<Token>();
            
            Lexem lex;
            while (true)
            {
                if(!iterator.MoveToContent())
                {
                    throw PreprocessorError("Непредусмотренное завершение текста модуля");
                }

                lex = _lexer.NextLexem();
                if (lex.Token == Token.Then)
                {
                    iterator.MoveToContent();
                    break;
                }
                
                if (lex.Type == LexemType.Operator)
                {
                    ProcessOperator(priority, expressionResult, orderedOperators, operators, lex);
                }
                else if (lex.Type == LexemType.Identifier)
                {
                    expressionResult.Add(IsDefined(lex.Content));
                }
                else
                {
                    throw PreprocessorError("Ошибка в выражении препроцессора");
                } 
            }

            while (operators.Count > 0)
            {
                var oper = operators.Pop();
                if (oper == Token.OpenPar)
                    throw PreprocessorError("Ошибка в выражении препроцессора");

                orderedOperators.Add(oper);
                expressionResult.Add(null);
            }

            Stack<bool> calculator = new Stack<bool>();

            for (int i = 0, j = 0; i < expressionResult.Count; i++)
            {
                if (expressionResult[i] != null)
                {
                    calculator.Push((bool)expressionResult[i]);
                }
                else
                {
                    var opCode = orderedOperators[j++];

                    switch (opCode)
                    {
                        case Token.Not:
                            var op = calculator.Pop();
                            calculator.Push(!op);
                            break;
                        case Token.And:
                            {
                                var op1 = calculator.Pop();
                                var op2 = calculator.Pop();
                                calculator.Push(op1 && op2);
                                break;
                            }
                        case Token.Or:
                            {
                                var op1 = calculator.Pop();
                                var op2 = calculator.Pop();
                                calculator.Push(op1 || op2);
                                break;
                            }
                    }
                }
            }

            return calculator.Pop();
            
        }

        private static void ProcessOperator(Dictionary<Token, int> priority, List<bool?> expressionResult, List<Token> orderedOperators, Stack<Token> operators, Lexem lex)
        {
            if (operators.Count == 0)
            {
                operators.Push(lex.Token);
                return;
            }
            
            var opOnStack = operators.Peek();
            if (opOnStack != Token.OpenPar)
            {
                var currentPriority = priority[lex.Token];
                var stackPriority = priority[opOnStack];

                while (currentPriority <= stackPriority)
                {
                    var stackOp = operators.Peek();
                    if (stackOp != Token.OpenPar)
                    {
                        operators.Pop();
                        orderedOperators.Add(stackOp);
                        expressionResult.Add(null);
                        if (operators.Count > 0)
                        {
                            stackOp = operators.Peek();

                            if (stackOp == Token.OpenPar)
                                break;

                            stackPriority = priority[stackOp];
                        }
                        else
                            break;
                    }
                    else
                    {
                        break;
                    }
                }

                operators.Push(lex.Token);

            }
            else
            {
                operators.Push(lex.Token);
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
                _lexer.Code = _code;
            }
        }

        public int CurrentColumn
        {
            get { return _lexer.CurrentColumn; }
        }

        public int CurrentLine
        {
            get { return _lexer.CurrentLine; }
        }

        public CodePositionInfo GetCodePosition()
        {
            return _lexer.GetCodePosition();
        }

        public Lexem NextLexem()
        {
            MoveNext();

            if (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
                return Preprocess(_lastExtractedLexem);

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
                var enterBlock = SolveExpression(_lexer.GetIterator());
                PushBlock(enterBlock);

                if (enterBlock)
                    return NextLexem();

                SkipTillNextDirective();

                return Preprocess(_lastExtractedLexem);

            }
            else if(directive.Token == Token.ElseIf)
            {
                if (BlockIsSolved())
                {
                    SkipTillNextDirective();
                    return Preprocess(_lastExtractedLexem);
                }
                else
                {
                    var enterBlock = SolveExpression(_lexer.GetIterator());
                    if (enterBlock)
                    {
                        MarkAsSolved();
                        return NextLexem();
                    }
                    else
                    {
                        SkipTillNextDirective();
                        return Preprocess(_lastExtractedLexem);
                    }
                }

            }
            else if(directive.Token == Token.Else)
            {
                SetOutput(!BlockIsSolved());

                if (OutputIsOn())
                    return NextLexem();
                else
                {
                    SkipTillNextDirective();
                    if (_lastExtractedLexem.Token != Token.EndIf)
                        throw PreprocessorError("Ожидается директива препроцессора #КонецЕсли");

                    return Preprocess(_lastExtractedLexem);
                }
            }
            else if(directive.Token == Token.EndIf)
            {
                PopBlock();
                return NextLexem();
            }

            HandleUnknownDirective(_lastExtractedLexem);
            
            return _lastExtractedLexem;
        }

        private void SkipTillNextDirective()
        {
            int currentLevel = BlockLevel();
            MoveNext();

            while (true)
            {
                while (_lastExtractedLexem.Type != LexemType.PreprocessorDirective)
                {
                    if (_lastExtractedLexem.Token == Token.EndOfText)
                        throw PreprocessorError("Ожидается директива препроцессора #КонецЕсли");

                    MoveNext();
                }

                if (_lastExtractedLexem.Token == Token.If)
                    PushBlock(false);
                else if (_lastExtractedLexem.Token == Token.EndIf && BlockLevel() > currentLevel)
                    PopBlock();
                else if (BlockLevel() == currentLevel)
                    break;

                MoveNext();

            }
        }

    }

    public class PreprocessorUnknownTokenEventArgs : EventArgs
    {
        public bool IsHandled { get; set; }
        public SourceCodeIterator Iterator { get; set; }
        public Lexem Lexem { get; set; }
    }
}
