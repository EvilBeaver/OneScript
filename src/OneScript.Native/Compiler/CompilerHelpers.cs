/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OneScript.Contexts;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Types;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    public static class CompilerHelpers
    {
        public static BslPrimitiveValue ValueFromLiteral(in Lexem lex)
        {
            return lex.Type switch
            {
                LexemType.NumberLiteral => BslNumericValue.Parse(lex.Content),
                LexemType.BooleanLiteral => BslBooleanValue.Parse(lex.Content),
                LexemType.StringLiteral => BslStringValue.Create(lex.Content),
                LexemType.DateLiteral => BslDateValue.Parse(lex.Content),
                LexemType.UndefinedLiteral => BslUndefinedValue.Instance,
                LexemType.NullLiteral => BslNullValue.Instance,
                _ => throw new NotImplementedException()
            };
        }
        
        public static object ClrValueFromLiteral(in Lexem lex)
        {
            return lex.Type switch
            {
                LexemType.NumberLiteral => (decimal)BslNumericValue.Parse(lex.Content),
                LexemType.BooleanLiteral => (bool)BslBooleanValue.Parse(lex.Content),
                LexemType.StringLiteral => (string)BslStringValue.Create(lex.Content),
                LexemType.DateLiteral => (DateTime)BslDateValue.Parse(lex.Content),
                LexemType.UndefinedLiteral => BslUndefinedValue.Instance,
                LexemType.NullLiteral => BslNullValue.Instance,
                _ => throw new NotImplementedException()
            };
        }

        public static IEnumerable<Attribute> GetAnnotations(IEnumerable<AnnotationNode> annotations)
        {
            // Возможно будут какие-то маппинги на системные атрибуты, не только на BslAnnotation
            // поэтому возвращаем Attribute[] а не BslAnnotation[]

            return annotations.Select(GetBslAnnotation).ToList();
        }
        
        public static BslAnnotationAttribute GetBslAnnotation(AnnotationNode node)
        {
            var parameters = GetAnnotationParameters(node);
            return new BslAnnotationAttribute(node.Name, parameters);
        }
        
        public static Type GetClrType(TypeDescriptor type)
        {
            Type clrType;
            if (type == BasicTypes.String)
                clrType = typeof(string);
            else if (type == BasicTypes.Date)
                clrType = typeof(DateTime);
            else if (type == BasicTypes.Boolean)
                clrType = typeof(bool);
            else if (type == BasicTypes.Number)
                clrType = typeof(decimal);
            else if (type == BasicTypes.Type)
                clrType = typeof(BslTypeValue);
            else
                clrType = type.ImplementingClass;

            return clrType;
        }
        
        private static IEnumerable<BslAnnotationParameter> GetAnnotationParameters(AnnotationNode node)
        {
            return node.Children.Cast<AnnotationParameterNode>()
                .Select(MakeAnnotationParameter)
                .ToList();
        }
        
        private static BslAnnotationParameter MakeAnnotationParameter(AnnotationParameterNode param)
        {
            BslAnnotationParameter result;
            if (param.Value.Type != LexemType.NotALexem)
            {
                var runtimeValue = ValueFromLiteral(param.Value);
                result = new BslAnnotationParameter(param.Name, runtimeValue);
            }
            else
            {
                result = new BslAnnotationParameter(param.Name, null);
            }

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetIdentifier(this BslSyntaxNode node)
        {
            return GetIdentifier((TerminalNode) node);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetIdentifier(this TerminalNode node)
        {
            return node.Lexem.Content;
        }
    }
}