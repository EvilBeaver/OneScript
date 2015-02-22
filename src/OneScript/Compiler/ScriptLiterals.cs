using OneScript.Compiler.Lexics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Compiler
{
    public struct ConstDefinition
    {
        public ConstType Type;
        public string Presentation;

        public static ConstDefinition CreateFromLiteral(ref Lexem lex)
        {
            System.Diagnostics.Debug.Assert(LanguageDef.IsLiteral(ref lex));

            ConstType constType;

            switch (lex.Type)
            {
                case LexemType.StringLiteral:
                    constType = ConstType.String;
                    break;
                case LexemType.UndefinedLiteral:
                    constType = ConstType.Undefined;
                    break;
                case LexemType.BooleanLiteral:
                    constType = ConstType.Boolean;
                    break;
                case LexemType.DateLiteral:
                    constType = ConstType.Date;
                    break;
                case LexemType.NumberLiteral:
                    constType = ConstType.Number;
                    break;
                case LexemType.NullLiteral:
                    constType = ConstType.Null;
                    break;
                default:
                    throw new InvalidOperationException("Lexem type is not a literal");
                
            }

            ConstDefinition cDef = new ConstDefinition()
            {
                Type = constType,
                Presentation = lex.Content
            };
            return cDef;
        }
    }

    public enum ConstType
    {
        NotAConstant,
        Undefined,
        String,
        Number,
        Boolean,
        Date,
        Null
    }
}
