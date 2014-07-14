using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
partial class Compiler
{
    class ExpressionBuilder
    {
        Compiler _compiler;
        Stack<Token> _operators;
        Stack<int> _logicalJumps;

        public ExpressionBuilder(Compiler compiler)
        {
            _compiler = compiler;
        }

        public void Build(Token stopToken)
        {
            const int STATE_UNDEF = 0;
            const int STATE_OPERAND_EXPECTED = 1;
            const int STATE_FUNC_WAIT = 2;
            const int STATE_OPERATOR_EXPECTED = 3;

            _operators = new Stack<Token>();

            int currentState = STATE_UNDEF;
            string waitingIdentifier = null;
            int parCount = 0;

            while (true)
            {
                bool success = false;

                if (_compiler._lastExtractedLexem.Token == stopToken)
                {
                    if (currentState == STATE_FUNC_WAIT)
                    {
                        _compiler.BuildPushVariable(waitingIdentifier);
                    }
                    else if (currentState != STATE_OPERATOR_EXPECTED)
                    {
                        throw CompilerException.ExpressionSyntax();
                    }
                    break;
                }

                switch (currentState)
                {
                    case STATE_UNDEF:
                    case STATE_OPERAND_EXPECTED:
                        if (_compiler.IsLiteral(ref _compiler._lastExtractedLexem))
                        {
                            _compiler.BuildPushConstant();
                            currentState = STATE_OPERATOR_EXPECTED;
                            success = true;
                        }
                        else if (_compiler._lastExtractedLexem.Type == LexemType.Identifier)
                        {
                            if (_compiler._lastExtractedLexem.Token == Token.NotAToken)
                            {
                                waitingIdentifier = _compiler._lastExtractedLexem.Content;
                                currentState = STATE_FUNC_WAIT;
                                success = true;
                            }
                            else if (LanguageDef.IsBuiltInFunction(_compiler._lastExtractedLexem.Token))
                            {
                                _compiler.BuildBuiltinFunction();
                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else if (_compiler._lastExtractedLexem.Token == Token.NewObject)
                            {
                                _compiler.BuildNewObjectCreation();
                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else
                            {
                                throw CompilerException.TokenExpected(stopToken);
                            }
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.Minus)
                        {
                            PushOperator(Token.UnaryMinus);
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.OpenPar)
                        {
                            PushOperator(Token.OpenPar);
                            parCount++;
                            currentState = STATE_UNDEF;
                            success = true;
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.Not)
                        {
                            PushOperator(Token.Not);
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }

                        break;
                    case STATE_FUNC_WAIT:
                        if (_compiler._lastExtractedLexem.Token == Token.OpenPar)
                        {
                            _compiler.BuildFunctionCall(waitingIdentifier);
                            if (_compiler._lastExtractedLexem.Token == Token.ClosePar)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else if (_compiler._lastExtractedLexem.Type == LexemType.Operator)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                continue;
                            }
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.OpenBracket)
                        {
                            _compiler.BuildIndexedAccess(waitingIdentifier);
                            currentState = STATE_OPERATOR_EXPECTED;
                            success = true;
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.Dot)
                        {
                            _compiler.BuildPushVariable(waitingIdentifier);
                            int identifier;
                            _compiler.BuildResolveChain(out identifier);
                            _compiler.ProcessResolvedItem(identifier, stopToken);

                            if (_compiler._lastExtractedLexem.Type == LexemType.Operator || _compiler._lastExtractedLexem.Token == stopToken)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                continue;
                            }
                        }
                        else if (_compiler._lastExtractedLexem.Type == LexemType.Operator && IsBinaryOperator(_compiler._lastExtractedLexem.Token))
                        {
                            _compiler.BuildPushVariable(waitingIdentifier);
                            currentState = STATE_OPERATOR_EXPECTED;
                            continue;
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.ClosePar)
                        {
                            _compiler.BuildPushVariable(waitingIdentifier);
                            currentState = STATE_OPERATOR_EXPECTED;
                            continue;
                        }

                        break;
                    case STATE_OPERATOR_EXPECTED:
                        if (_compiler._lastExtractedLexem.Type == LexemType.Operator && IsBinaryOperator(_compiler._lastExtractedLexem.Token))
                        {
                            ProcessExpressionOperator();
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.ClosePar)
                        {
                            Token current;
                            parCount--;
                            if (parCount < 0)
                            {
                                UnwindOperators();
                                var cp = new CodePositionInfo();
                                cp.LineNumber = _compiler._parser.CurrentLine;
                                cp.Code = _compiler._parser.GetCodeLine(cp.LineNumber);
                                throw new ExtraClosedParenthesis(cp);
                            }

                            if (_operators.Count > 0)
                            {
                                while ((current = PopOperator()) != Token.OpenPar)
                                {
                                    AddCommandForToken(current);
                                    if (_operators.Count == 0)
                                    {
                                        throw CompilerException.TokenExpected(Token.OpenPar);
                                    }
                                }

                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else
                            {
                                throw CompilerException.TokenExpected(Token.OpenPar);
                            }
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.OpenBracket)
                        {
                            _compiler.NextToken();
                            _compiler.BuildExpression(Token.CloseBracket);
                            _compiler.AddCommand(OperationCode.PushIndexed, 0);
                            currentState = STATE_OPERATOR_EXPECTED;
                            success = true;
                        }
                        else if (_compiler._lastExtractedLexem.Token == Token.Dot)
                        {
                            int identifier;
                            _compiler.BuildResolveChain(out identifier);
                            _compiler.ProcessResolvedItem(identifier, stopToken);

                            if (_compiler._lastExtractedLexem.Type == LexemType.Operator || _compiler._lastExtractedLexem.Token == stopToken)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                continue;
                            }

                        }
                        else if (_compiler._lastExtractedLexem.Token != stopToken)
                        {
                            throw CompilerException.TokenExpected(stopToken);
                        }
                        break;
                }

                if (success)
                {
                    if (_compiler._lastExtractedLexem.Token != stopToken)
                        _compiler.NextToken();
                }
                else
                {
                    throw CompilerException.ExpressionSyntax();
                }
            }

            UnwindOperators();
        }

        private bool IsBinaryOperator(Token token)
        {
            return token == Token.Plus
                || token == Token.Minus
                || token == Token.Multiply
                || token == Token.Division
                || token == Token.Modulo
                || token == Token.And
                || token == Token.Or
                || token == Token.Not
                || token == Token.LessThan
                || token == Token.LessOrEqual
                || token == Token.MoreThan
                || token == Token.MoreOrEqual
                || token == Token.Equal
                || token == Token.NotEqual;
        }

        private bool IsLogicalOperator(Token token)
        {
            return token == Token.And || token == Token.Or;
        }

        private void AddCommandForToken(Token current)
        {
            if (IsLogicalOperator(current))
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(HasPendingLogicalJumps());
#endif
                _compiler.AddCommand(OperationCode.MakeBool, 0);
                var idx = LogicalJumps.Pop();
                _compiler.CorrectCommandArgument(idx, _compiler._module.Code.Count);
            }
            else
            {
                _compiler.AddCommand(TokenToOperationCode(current), 0);
            }
        }

        private void ProcessExpressionOperator()
        {
            if (_operators.Count == 0)
            {
               PushOperator(_compiler._lastExtractedLexem.Token);
                return;
            }

            var opOnStack = _operators.Peek();
            if (opOnStack != Token.OpenPar)
            {
                var currentPriority = LanguageDef.GetPriority(_compiler._lastExtractedLexem.Token);
                var stackPriority = LanguageDef.GetPriority(opOnStack);

                while (stackPriority >= currentPriority && _operators.Count > 0)
                {
                    var stackOp = _operators.Peek();
                    if (stackOp != Token.OpenPar)
                    {
                        PopOperator();
                        AddCommandForToken(stackOp);
                    }
                    else
                    {
                        break;
                    }
                }

                PushOperator(_compiler._lastExtractedLexem.Token);

            }
            else
            {
               PushOperator(_compiler._lastExtractedLexem.Token);
            }
        }

        private void PushOperator(Token token)
        {
            _operators.Push(token);
            if (IsLogicalOperator(token))
            {
                var jmpIdx = _compiler.AddCommand(TokenToOperationCode(token), -1);
                LogicalJumps.Push(jmpIdx);
            }
        }

        private Token PopOperator()
        {
            return _operators.Pop();
        }

        private void UnwindOperators()
        {
            while (_operators.Count > 0)
            {
                var oper = PopOperator();
                if (oper == Token.OpenPar)
                    throw CompilerException.ExpressionSyntax();

                AddCommandForToken(oper);
            }
        }

        private static OperationCode TokenToOperationCode(Token stackOp)
        {
            OperationCode opCode;
            switch (stackOp)
            {
                case Token.Equal:
                    opCode = OperationCode.Equals;
                    break;
                case Token.NotEqual:
                    opCode = OperationCode.NotEqual;
                    break;
                case Token.Plus:
                    opCode = OperationCode.Add;
                    break;
                case Token.Minus:
                    opCode = OperationCode.Sub;
                    break;
                case Token.Multiply:
                    opCode = OperationCode.Mul;
                    break;
                case Token.Division:
                    opCode = OperationCode.Div;
                    break;
                case Token.Modulo:
                    opCode = OperationCode.Mod;
                    break;
                case Token.UnaryMinus:
                    opCode = OperationCode.Neg;
                    break;
                case Token.And:
                    opCode = OperationCode.And;
                    break;
                case Token.Or:
                    opCode = OperationCode.Or;
                    break;
                case Token.Not:
                    opCode = OperationCode.Not;
                    break;
                case Token.LessThan:
                    opCode = OperationCode.Less;
                    break;
                case Token.LessOrEqual:
                    opCode = OperationCode.LessOrEqual;
                    break;
                case Token.MoreThan:
                    opCode = OperationCode.Greater;
                    break;
                case Token.MoreOrEqual:
                    opCode = OperationCode.GreaterOrEqual;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return opCode;
        }

        private Stack<int> LogicalJumps
        {
            get
            {
                if (_logicalJumps == null)
                {
                    _logicalJumps = new Stack<int>();
                }

                return _logicalJumps;
            }
        }

#if DEBUG
        public bool HasPendingLogicalJumps()
        {
            return _logicalJumps == null ? false : _logicalJumps.Count > 0;
        }
#endif

    }
}
}
