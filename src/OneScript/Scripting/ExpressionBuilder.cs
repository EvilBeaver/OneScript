using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class ExpressionBuilder
    {
        enum BuilderState
        {
            Unknown,
            OperandExpected,
            OperatorExpected,
            MethodCall
        }

        IModuleBuilder _builder;
        ILexemExtractor _lexemSrc;
        Lexem _lastExtractedLexem;
        BuilderState _state;
        int _parCount = 0;
        ICompilerContext _context;
        

        public ExpressionBuilder(IModuleBuilder builder, ILexemExtractor lexemSource, ICompilerContext context)
        {
            _builder = builder;
            _lexemSrc = lexemSource;
            _context = context;
            _lastExtractedLexem = lexemSource.LastExtractedLexem;
        }

        public void Build(Token stopToken)
        {
            while(_lastExtractedLexem.Token != stopToken)
            {
                if (_lastExtractedLexem.Token == Token.EndOfText)
                    break;

                switch(_state)
                {
                    case BuilderState.Unknown:
                    case BuilderState.OperandExpected:
                        ProcessOperand();
                        NextLexem();
                        break;
                    case BuilderState.OperatorExpected:
                        ProcessOperator();
                        NextLexem();
                        break;
                }

            }
        }

        private void ProcessOperand()
        {
            Debug.Assert(_state == BuilderState.OperandExpected || (_state == BuilderState.Unknown));

            if (LanguageDef.IsIdentifier(ref _lastExtractedLexem))
            {
                BeginOperandIdentifier();
            }
            else
            {
                BeginOperandNonIdentifier();
            }

        }

        private void ProcessOperator()
        {
            Debug.Assert(_state == BuilderState.OperatorExpected);
            
            if(IsBinaryOperator(_lastExtractedLexem.Token))
            {
                _builder.AddOperation(_lastExtractedLexem.Token);
            }
            else if (_lastExtractedLexem.Token == Token.ClosePar)
            {
                //ProcessClosedParenthesis();
            }
            else if (_lastExtractedLexem.Token == Token.CloseBracket)
            {
                //ProcessClosedBracket();
            }
            else
            {
                throw CompilerException.ExpressionSyntax();
            }

        }

        private void BeginOperandIdentifier()
        {
            var identifier = _lastExtractedLexem.Content;
            
            NextLexem();

            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                _builder.BeginMethodCall(identifier, true);
                _parCount++;
                _state = BuilderState.MethodCall;
            }
            else if (_lastExtractedLexem.Token == Token.OpenBracket)
            {

            }
            else if (_lastExtractedLexem.Token == Token.Dot)
            {

            }
            else if (IsBinaryOperator(_lastExtractedLexem.Token))
            {
                _builder.BuildReadVariable(_context.GetVariable(identifier));
                _builder.AddOperation(_lastExtractedLexem.Token);
                _state = BuilderState.OperandExpected;
            }
            else if(_lastExtractedLexem.Token == Token.ClosePar)
            {
                //ProcessClosedParenthesis();
            }
            else if (_lastExtractedLexem.Token == Token.CloseBracket)
            {
                //ProcessClosedBracket();
            }
            else
            {
                throw CompilerException.ExpressionSyntax();
            }
        }

        private void BeginOperandNonIdentifier()
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                _builder.AddOperation(Token.OpenPar);
                _parCount++;
                _state = BuilderState.Unknown;
            }
            else if (_lastExtractedLexem.Token == Token.Minus)
            {
                _builder.AddOperation(Token.UnaryMinus);
                _state = BuilderState.OperandExpected;
            }
            else if (LanguageDef.IsLiteral(ref _lastExtractedLexem))
            {
                var constDef = Parser.CreateConstDefinition(ref _lastExtractedLexem);
                _builder.BuildReadConstant(constDef);
                _state = BuilderState.OperatorExpected;
            }
            else
            {
                throw CompilerException.ExpressionSyntax();
            }
        }

        private void NextLexem()
        {
            _lexemSrc.NextLexem();
            _lastExtractedLexem = _lexemSrc.LastExtractedLexem;
        }

        private static bool IsBinaryOperator(Token token)
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

        private static bool IsLogicalOperator(Token token)
        {
            return token == Token.And || token == Token.Or;
        }
    }
}
