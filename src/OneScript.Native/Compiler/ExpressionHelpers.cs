/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using OneScript.Language.LexicalAnalysis;
using OneScript.Native.Runtime;

namespace OneScript.Native.Compiler
{
    public static class ExpressionHelpers
    {
        public static Expression DowncastDecimal(Expression decimalValue, Type targetType)
        {
            return Expression.Convert(decimalValue, targetType);
        }

        // public static Type GetClrType(TypeDescriptor type)
        // {
        //     Type clrType;
        //     if (type == BasicTypes.String)
        //         clrType = typeof(string);
        //     else if (type == BasicTypes.Date)
        //         clrType = typeof(DateTime);
        //     else if (type == BasicTypes.Boolean)
        //         clrType = typeof(bool);
        //     else if (type == BasicTypes.Number)
        //         clrType = typeof(decimal);
        //     else if (type == BasicTypes.Type)
        //         clrType = typeof(TypeTypeValue);
        //     else
        //         clrType = type.ImplementingClass;
        //
        //     return clrType;
        // }

        public static ExpressionType TokenToOperationCode(Token stackOp)
        {
            ExpressionType opCode;
            switch (stackOp)
            {
                case Token.Equal:
                    opCode = ExpressionType.Equal;
                    break;
                case Token.NotEqual:
                    opCode = ExpressionType.NotEqual;
                    break;
                case Token.And:
                    opCode = ExpressionType.And;
                    break;
                case Token.Or:
                    opCode = ExpressionType.Or;
                    break;
                case Token.Plus:
                    opCode = ExpressionType.Add;
                    break;
                case Token.Minus:
                    opCode = ExpressionType.Subtract;
                    break;
                case Token.Multiply:
                    opCode = ExpressionType.Multiply;
                    break;
                case Token.Division:
                    opCode = ExpressionType.Divide;
                    break;
                case Token.Modulo:
                    opCode = ExpressionType.Modulo;
                    break;
                case Token.UnaryPlus:
                    opCode = ExpressionType.UnaryPlus;
                    break;
                case Token.UnaryMinus:
                    opCode = ExpressionType.Negate;
                    break;
                case Token.Not:
                    opCode = ExpressionType.Not;
                    break;
                case Token.LessThan:
                    opCode = ExpressionType.LessThan;
                    break;
                case Token.LessOrEqual:
                    opCode = ExpressionType.LessThanOrEqual;
                    break;
                case Token.MoreThan:
                    opCode = ExpressionType.GreaterThan;
                    break;
                case Token.MoreOrEqual:
                    opCode = ExpressionType.GreaterThanOrEqual;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return opCode;
        }

        private static ReflectedMethodsCache _operationsCache = new ReflectedMethodsCache();
        
        public static Expression ToBoolean(Expression right)
        {
            if (right.Type == typeof(bool))
                return right;
            
            var method = _operationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ToBoolean));

            return Expression.Call(method, right);
        }
        
        public static Expression ToNumber(Expression right)
        {
            if (right.Type == typeof(decimal))
                return right;
            
            var method = _operationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ToNumber));

            return Expression.Call(method, right);
        }
        
        public static Expression ToDate(Expression right)
        {
            if (right.Type == typeof(DateTime))
                return right;
            
            var method = _operationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ToDate));

            return Expression.Call(method, right);
        }
        
        public static Expression ToString(Expression right)
        {
            var method = _operationsCache.GetOrAdd(
                typeof(object),
                nameof(object.ToString),
                BindingFlags.Public | BindingFlags.Instance);

            return Expression.Call(right, method);
        }

        public static Expression Add(Expression left, Expression right)
        {
            var operation = _operationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.Add));

            return Expression.Call(operation, left, right);
        }
        
        public static Expression Subtract(Expression left, Expression right)
        {
            var operation = _operationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.Subtract));

            return Expression.Call(operation, left, right);
        }
    }
}