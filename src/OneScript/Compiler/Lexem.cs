using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Compiler
{
    public struct Lexem
    {
        public LexemType Type;
        public string Content;
        public Token Token;

        public static Lexem Empty()
        {
            return new Lexem() { Type = LexemType.NotALexem };
        }

        public static Lexem EndOfText()
        {
            return new Lexem() { Type = LexemType.EndOfText, Token = Token.EndOfText };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Enum.GetName(typeof(LexemType), this.Type), Content);
        }

    }

    public enum LexemType
    {
        NotALexem,
        Identifier,
        Operator,
        StringLiteral,
        DateLiteral,
        NumberLiteral,
        BooleanLiteral,
        UndefinedLiteral,
        NullLiteral,
        PreprocessorDirective,
        EndOperator,
        EndOfText
    }

}
