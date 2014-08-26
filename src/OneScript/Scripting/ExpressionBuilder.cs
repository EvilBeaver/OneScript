using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class ExpressionBuilder
    {
        IModuleBuilder _builder;
        Stack<Token> _operators = new Stack<Token>();
        ILexemExtractor _lexemSrc;

        Lexem _lastExtractedLexem;

        public ExpressionBuilder(IModuleBuilder builder, ILexemExtractor lexemSource)
        {
            _builder = builder;
            _lexemSrc = lexemSource;
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
