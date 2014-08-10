using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class Preprocessor
    {
        enum PreprocessorState
        {
            Off,
            IfBlock,
            ElseIfBlock,
            ElseBlock,
            Solved,
            RequireEnd
        };

        HashSet<string> _definitions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        PreprocessorState _currentState = PreprocessorState.Off;
        bool _isExcluding = false;
        WordLexerState _wordExtractor = new WordLexerState();
        int _level = 0;

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

        public bool Solve(SourceCodeIterator iterator)
        {
            System.Diagnostics.Debug.Assert(iterator.CurrentSymbol == '#');

            if (_isExcluding && _level > 1)
                return false;

            bool result;

            switch(_currentState)
            {
                case PreprocessorState.Off:
                    result = SolveIfBlock(iterator);
                    break;
                case PreprocessorState.IfBlock:
                case PreprocessorState.ElseIfBlock:
                    result = DispatchElseVariants(iterator);
                    break;
                case PreprocessorState.Solved:
                    result = SkipAllBranches(iterator);
                    break;
                case PreprocessorState.RequireEnd:
                    result = TryEndPreprocessing(iterator);
                    break;
                default:
                    throw PreprocessorError(iterator, "Ожидается директива: " + Token.EndIf);
            }

            iterator.MoveToContent();
            return result;

        }

        private bool SolveIfBlock(SourceCodeIterator iterator)
        {
            _level++;
            var lex = NextLexem(iterator);
            
            CheckExpectedToken(iterator, lex.Token, Token.If);

            _currentState = PreprocessorState.IfBlock;
            var solved = SolveExpression(iterator);
            if(solved)
            {
                _currentState = PreprocessorState.Solved;
                _isExcluding = false;
            }
            else
            {
                _isExcluding = true;
            }

            return !_isExcluding;
        }

        private bool DispatchElseVariants(SourceCodeIterator iterator)
        {
            var lex = NextLexem(iterator);

            CheckExpectedToken(iterator, lex.Token, Token.ElseIf, Token.Else, Token.EndIf);
            if(lex.Token == Token.ElseIf)
            {
                _currentState = PreprocessorState.ElseIfBlock;
                var solved = SolveExpression(iterator);
                if (solved)
                {
                    _currentState = PreprocessorState.Solved;
                    _isExcluding = false;
                }
                else
                {
                    _isExcluding = true;
                }

                return !_isExcluding;
            }
            else if(lex.Token == Token.Else)
            {
                _isExcluding = false;
                _currentState = PreprocessorState.RequireEnd;
                return true;

            }
            else
            {
                return EndPreprocessing();
            }
        }
        private bool TryEndPreprocessing(SourceCodeIterator iterator)
        {
            var lex = NextLexem(iterator);
            CheckExpectedToken(iterator, lex.Token, Token.EndIf);

            return EndPreprocessing();
        }

        private bool SkipAllBranches(SourceCodeIterator iterator)
        {
            var lex = NextLexem(iterator);
            if(lex.Token == Token.EndIf)
            {
                return EndPreprocessing();
            }
            else if(lex.Token == Token.ElseIf)
            {
                SolveExpression(iterator);
            }
            return false;
        }

        private bool EndPreprocessing()
        {
            if (--_level == 0)
            {
                _currentState = PreprocessorState.Off;
                _isExcluding = false;
            }

            return !_isExcluding;

        }

        private void CheckExpectedToken(SourceCodeIterator iterator, Token real, params Token[] expected)
        {
            if (!expected.Contains(real))
            {
                var sb = new StringBuilder();
                foreach (var item in expected)
                {
                    sb.Append(item);
                    sb.Append('/');
                }
                throw PreprocessorError(iterator, "Ожидается директива препроцессора: " + sb.ToString());
            }
        }

        private static SyntaxErrorException PreprocessorError(SourceCodeIterator iterator, string message)
        {
            return new SyntaxErrorException(iterator.GetPositionInfo(), message);
        }

        
        private Lexem NextLexem(SourceCodeIterator iterator)
        {
            if(iterator.MoveNext() && iterator.MoveToContent())
            {
                return _wordExtractor.ReadNextLexem(iterator);
            }
            else
            {
                throw PreprocessorError(iterator, "Непредусмотренное завершение текста");
            }
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
                    throw PreprocessorError(iterator, "Непредусмотренное завершение текста модуля");
                }

                lex = _wordExtractor.ReadNextLexem(iterator);
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
                    throw PreprocessorError(iterator, "Ошибка в выражении препроцессора");
                } 
            }

            while (operators.Count > 0)
            {
                var oper = operators.Pop();
                if (oper == Token.OpenPar)
                    throw PreprocessorError(iterator, "Ошибка в выражении препроцессора");

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

        public void End()
        {
            if(_currentState != PreprocessorState.Off)
            {
                throw new SyntaxErrorException(new CodePositionInfo(), "Ожидается директива #КонецЕсли/#EndIf");
            }
            _level = 0;
        }
    }
}
