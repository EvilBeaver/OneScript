/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;
using MethodInfo = System.Reflection.MethodInfo;

namespace OneScript.StandardLibrary.Native
{
    public static class ExpressionHelpers
    {
        public static Expression CallToGetPropertyProtocol(IRuntimeContextInstance target, string propertyName)
        {
            var helperMethod = typeof(RCIHelperExtensions).GetMethod(nameof(RCIHelperExtensions.GetPropValue));
            Debug.Assert(helperMethod != null);

            return Expression.Call(null, helperMethod,
                Expression.Constant(target),
                Expression.Constant(propertyName));
        }
        
        public static Expression CallToSetPropertyProtocol(IRuntimeContextInstance target, string propertyName, Expression value)
        {
            var helperMethod = typeof(RCIHelperExtensions).GetMethod(nameof(RCIHelperExtensions.SetPropValue));
            Debug.Assert(helperMethod != null);

            return Expression.Call(null, helperMethod,
                Expression.Constant(target),
                Expression.Constant(propertyName),
                value);
        }

        public static Expression ConvertFromIValue(Expression source, Type targetType)
        {
            return Expression.Convert(
                Expression.Call(null, MarshallerFromIValue, source, Expression.Constant(targetType)),
                targetType);
        }

        public static Expression CastToClrObject(Expression source, Type backCast)
        {
            return Expression.Convert(Expression.Call(null, CastToObject, source), backCast);
        }
        
        public static Expression CastToClrObject(Expression source)
        {
            return Expression.Call(null, CastToObject, source);
        }
        
        public static Expression ConvertToIValue(Expression source)
        {
            var arg = source.Type.IsValueType? Expression.Convert(source, typeof(object)) : source;
            
            return Expression.Call(null, MarshallerToIValue, arg, Expression.Constant(source.Type));
        }

        public static Expression ToNumber(Expression source)
        {
            if (source.Type == typeof(decimal))
                return source;
            
            var methodInfo = source.Type.GetConversionMethod(typeof(decimal), "AsNumber");
            return Expression.Call(source, methodInfo);
        }
        
        public static Expression ToString(Expression source)
        {
            if (source.Type == typeof(string))
                return source;
            
            var methodInfo = source.Type.GetConversionMethod(typeof(string), "AsString");
            return Expression.Call(source, methodInfo);
        }
        
        public static Expression ToBoolean(Expression source)
        {
            if (source.Type == typeof(bool))
                return source;
            
            var methodInfo = source.Type.GetConversionMethod(typeof(bool), "AsBoolean");
            return Expression.Call(source, methodInfo);
        }
        
        public static Expression ToDate(Expression source)
        {
            if (source.Type == typeof(DateTime))
                return source;
            
            var methodInfo = source.Type.GetConversionMethod(typeof(DateTime), "AsDate");
            return Expression.Call(source, methodInfo);
        }

        private static MethodInfo GetConversionMethod(this Type type, Type targetType, string methodName)
        {
            lock (_typeCasts)
            {
                if (!_typeCasts.TryGetValue(targetType, out var mi))
                {
                    mi = type.GetMethod(methodName);
                    Debug.Assert(mi != null);
                    _typeCasts[targetType] = mi;
                }

                return mi;
            }
        }

        public static Expression GetIValueSystemType(Expression value)
        {
            var typeProp = value.Type.GetProperty(nameof(IValue.SystemType));
            Debug.Assert(typeProp != null);
            
            return Expression.Property(value, typeProp);
        }
        
        public static Expression GetIValueClrType(Expression type)
        {
            var typeGetter = typeof(ExpressionHelpers).GetMethod(nameof(GetClrType));
            Debug.Assert(typeGetter != null);
            
            return Expression.Call(null, typeGetter, type);
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
                clrType = typeof(TypeTypeValue);
            else
                clrType = type.ImplementingClass;

            return clrType;
        }

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
        
        private static readonly Lazy<MethodInfo> _converterFromIValue;
        
        private static readonly Lazy<MethodInfo> _converterToIValue;
        
        private static readonly Lazy<MethodInfo> _castToObject;

        private static readonly Dictionary<Type, MethodInfo> _typeCasts = new Dictionary<Type, MethodInfo>();
        
        static ExpressionHelpers()
        {
            _converterFromIValue = new Lazy<MethodInfo>(() => typeof(ContextValuesMarshaller).GetMethod(
                nameof(ContextValuesMarshaller.ConvertParam), new[] {typeof(IValue), typeof(Type)}));

            _converterToIValue = new Lazy<MethodInfo>(() => typeof(ContextValuesMarshaller).GetMethod(
                nameof(ContextValuesMarshaller.ConvertReturnValue), new[] {typeof(object), typeof(Type)}));
            
            _castToObject = new Lazy<MethodInfo>(() => typeof(ContextValuesMarshaller).GetMethod(
                nameof(ContextValuesMarshaller.CastToCLRObject), 0, new[] {typeof(IValue)}));
        }

        public static MethodInfo MarshallerFromIValue => _converterFromIValue.Value;

        public static MethodInfo MarshallerToIValue => _converterToIValue.Value;

        public static MethodInfo CastToObject => _castToObject.Value;

    }
}