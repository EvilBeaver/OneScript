using OneScript.Scripting.Compiler.Lexics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler
{
    public struct ConstDefinition
    {
        public ConstType Type;
        public string Presentation;

        public static ConstDefinition CreateFromLiteral(ref Lexem lex)
        {
            System.Diagnostics.Debug.Assert(LanguageDef.IsLiteral(ref lex));

            ConstType constType = ConstType.Undefined;
            switch (lex.Type)
            {
                case LexemType.BooleanLiteral:
                    constType = ConstType.Boolean;
                    break;
                case LexemType.DateLiteral:
                    constType = ConstType.Date;
                    break;
                case LexemType.NumberLiteral:
                    constType = ConstType.Number;
                    break;
                case LexemType.StringLiteral:
                    constType = ConstType.String;
                    break;
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
        Date
    }
}
