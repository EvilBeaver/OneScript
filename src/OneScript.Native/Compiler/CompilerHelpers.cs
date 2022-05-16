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
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
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
                _ => throw new NotImplementedException()
            };
        }

        public static IEnumerable<object> GetAnnotations(IEnumerable<AnnotationNode> annotations)
        {
            // Возможно будут какие-то маппинги на системные атрибуты, не только на BslAnnotation
            // поэтому возвращаем object[] а не BslAnnotation[]

            var mappedAnnotations = new List<object>();
            foreach (var node in annotations)
            {
                var anno = new BslAnnotationAttribute(node.Name);
                anno.SetParameters(GetAnnotationParameters(node));
                mappedAnnotations.Add(anno);
            }

            return mappedAnnotations;
        }
        
        private static IEnumerable<BslAnnotationParameter> GetAnnotationParameters(AnnotationNode node)
        {
            return node.Children.Cast<AnnotationParameterNode>()
                .Select(param => new BslAnnotationParameter(param.Name, ValueFromLiteral(param.Value)))
                .ToList();
        }

        public static SymbolScope AddVariable(this SymbolScope scope, string name, Type type)
        {
            var symbol = new VariableSymbol
            {
                Name = name,
                Type = type
            };
        
            scope.Variables.Add(symbol, name);

            return scope;
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