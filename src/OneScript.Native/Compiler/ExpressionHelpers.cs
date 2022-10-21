/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using OneScript.Contexts;
using OneScript.Language.LexicalAnalysis;
using OneScript.Localization;
using OneScript.Native.Runtime;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Native.Compiler
{
    public static class ExpressionHelpers
    {
        public static Expression DowncastDecimal(Expression decimalValue, Type targetType)
        {
            return Expression.Convert(decimalValue, targetType);
        }
        
        public static Expression CastToDecimal(Expression value)
        {
            return Expression.Convert(value, typeof(decimal));
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
                    opCode = ExpressionType.AndAlso;
                    break;
                case Token.Or:
                    opCode = ExpressionType.OrElse;
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

        private static readonly ReflectedMethodsCache OperationsCache = new ReflectedMethodsCache();
        private static readonly ReflectedPropertiesCache PropertiesCache = new ReflectedPropertiesCache();
        
        public static Expression ToBoolean(Expression right)
        {
            if (right.Type == typeof(bool))
                return right;
            
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ToBoolean));

            return Expression.Call(method, right);
        }
        
        public static Expression ToNumber(Expression right)
        {
            if (right.Type == typeof(decimal))
                return right;
            
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ToNumber));

            return Expression.Call(method, right);
        }
        
        public static Expression ToDate(Expression right)
        {
            if (right.Type == typeof(DateTime))
                return right;
            
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ToDate));

            return Expression.Call(method, right);
        }
        
        public static Expression ToString(Expression right)
        {
            var method = OperationsCache.GetOrAdd(
                typeof(object),
                nameof(object.ToString),
                BindingFlags.Public | BindingFlags.Instance);

            return Expression.Call(right, method);
        }

        public static Expression Add(Expression left, Expression right)
        {
            var operation = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.Add));

            return Expression.Call(operation, left, ConvertToBslValue(right));
        }
        
        public static Expression Subtract(Expression left, Expression right)
        {
            var operation = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.Subtract));

            return Expression.Call(operation, left, ConvertToBslValue(right));
        }

        public static Expression ConvertToType(Expression value, Type targetType)
        {
            if (!TryStaticConversion(value, targetType, out var result))
            {
                return ConvertThroughBslValue(value, targetType);
            }

            return result;
        }

        private static Expression ConvertThroughBslValue(Expression value, Type targetType)
        {
            var bslVal = ConvertToBslValue(value);
            return ConvertToType(bslVal, targetType);
        }

        public static bool TryStaticConversion(Expression value, Type targetType, out Expression result)
        {
            if (targetType == typeof(BslValue) || targetType == typeof(IValue))
            {
                result = ConvertToBslValue(value);
                return true;
            }
            else if (typeof(BslObjectValue).IsAssignableFrom(targetType) && value.Type == typeof(BslUndefinedValue))
            {
                // это присваивание Неопределено
                // в переменную строго типизированного объекта
                // просто обнуляем переменную, не меняя тип на Неопределено
                result = Expression.Default(targetType);
                return true;
            }
            else
            {
                var conversion = TryFindConversionOp(value, targetType);
                if (conversion != null)
                {
                    result = conversion;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static Expression TryFindConversionOp(Expression value, Type targetType)
        {
            if (value.Type.IsValue())
            {
                if (targetType.IsNumeric())
                {
                    var decimalNum = ToNumber(value);
                    if (targetType.IsAssignableFrom(typeof(decimal)))
                        return decimalNum;

                    return DowncastDecimal(decimalNum, targetType);
                }
                if (targetType == typeof(bool))
                    return ToBoolean(value);
                if (targetType == typeof(string))
                    return ToString(value);
                if (targetType == typeof(DateTime))
                    return ToDate(value);
            }
            
            // пока в тупую делаем каст, а вдруг повезет
            // если будет ненадежно - поиграем с поиском статических конверсий
            try
            {
                return Expression.Convert(value, targetType);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static Expression DynamicallyCastToClrType(Expression value, Type targetType)
        {
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.Convert(
                CSharpBinderFlags.ConvertExplicit,
                targetType,
                value.Type);

            return Expression.Dynamic(binder, targetType, value);
        }

        public static Expression DynamicGetIndex(Expression target, Expression index)
        {
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetIndex(
                CSharpBinderFlags.ResultIndexed,
                target.Type,
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });

            return Expression.Dynamic(binder, typeof(object), target, index);
        }
        
        public static Expression DynamicSetIndex(Expression target, Expression index, Expression value)
        {
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.SetIndex(
                CSharpBinderFlags.ResultIndexed,
                target.Type,
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });

            return Expression.Dynamic(binder, typeof(object), target, index, value);
        }
        
        /// <summary>
        /// Пытается найти статический конвертер из типа в тип при передаче параметра
        /// </summary>
        /// <param name="parameter">параметр</param>
        /// <param name="targetType">тип аргумента</param>
        /// <returns>null если статический каст не удался, выражение конверсии, если удался</returns>
        public static Expression TryConvertParameter(Expression parameter, Type targetType)
        {
            if (targetType.IsAssignableFrom(parameter.Type))
                return parameter;

            if (targetType.IsNumeric() && parameter.Type.IsNumeric())
            {
                return DowncastDecimal(parameter, targetType);
            }

            if (targetType == typeof(string))
            {
                return ToString(parameter);
            }
            
            return ConvertToType(parameter, targetType);
        }

        public static Expression CallCompareTo(Expression target, Expression argument)
        {
            var compareToMethod = OperationsCache.GetOrAdd(
                typeof(IComparable<BslValue>),
                nameof(IComparable<BslValue>.CompareTo),
                BindingFlags.Instance | BindingFlags.Public
            );

            var bslArgument = ConvertToBslValue(argument);

            return Expression.Call(target, compareToMethod, bslArgument);
        }
        
        public static Expression CallEquals(Expression target, Expression argument)
        {
            var equalsMethod = OperationsCache.GetOrAdd(
                typeof(IEquatable<BslValue>),
                nameof(IEquatable<BslValue>.Equals),
                BindingFlags.Instance | BindingFlags.Public
            );

            var bslArgument = ConvertToBslValue(argument);

            return Expression.Call(target, equalsMethod, bslArgument);
        }
        
        public static Expression ConvertToBslValue(Expression value)
        {
            if (value.Type.IsValue())
                return value;
            
            var factoryClass = GetValueFactoryType(value.Type);
            if (factoryClass == null)
            {
                if (value.Type == typeof(object))
                {
                    // это результат динамической операции
                    // просто верим, что он BslValue
                    var meth = OperationsCache.GetOrAdd(
                        typeof(DynamicOperations),
                        nameof(DynamicOperations.WrapClrObjectToValue));
                    return Expression.Call(meth, value);
                }
                throw new NativeCompilerException(new BilingualString(
                    $"Преобразование из типа {value.Type} в тип BslValue не поддерживается",
                    $"Conversion from type {value.Type} into BslValue is not supported"));
            }
            
            if (value.Type == typeof(int) ||
                value.Type == typeof(long) ||
                value.Type == typeof(double))
            {
                value = CastToDecimal(value);
            }
            
            var factory = OperationsCache.GetOrAdd(factoryClass, "Create");
            return Expression.Call(factory, value);
        }

        private static Type GetValueFactoryType(Type clrType)
        {
            Type factoryClass = default;
            if (clrType == typeof(string))
            {
                factoryClass = typeof(BslStringValue);
            }
            else if (clrType == typeof(bool))
            {
                factoryClass = typeof(BslBooleanValue);
            }
            else if (clrType == typeof(decimal))
            {
                factoryClass = typeof(BslNumericValue);
            }
            else if (clrType == typeof(int) ||
                     clrType == typeof(long) ||
                     clrType == typeof(double))
            {
                factoryClass = typeof(BslNumericValue);
            }
            else if (clrType == typeof(DateTime))
            {
                factoryClass = typeof(BslDateValue);
            }

            return factoryClass;
        }

        public static bool IsInteger(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    return true;
                default:
                    return false;
            }
        }

        public static Expression CreateAssignmentSource(Expression source, Type targetType)
        {
            if (targetType == typeof(BslValue) && source.Type.IsValue())
            {
                // это универсальный вариант - не нужны конверсии
                return source;
            }
            
            // возможно прямое clr-присваивание
            if (targetType.IsAssignableFrom(source.Type))
                return source;

            var canBeCasted = TryStaticConversion(source, targetType, out var conversion);
            if (canBeCasted)
                return conversion;
            
            throw new NativeCompilerException(new BilingualString(
                $"Преобразование из типа {source.Type} в тип {targetType} не поддерживается",
                $"Conversion from type {source.Type} into {targetType} is not supported"));
        }

        public static Expression ConstructorCall(ITypeManager typeManager, Expression services, Expression type,
            Expression[] argsArray)
        {
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.ConstructorCall));

            var arrayOfArgs = Expression.NewArrayInit(typeof(BslValue), argsArray.Select(ConvertToBslValue));
            
            return Expression.Call(method, 
                Expression.Constant(typeManager),
                services,
                type,
                arrayOfArgs);
        }
        
        public static Expression ConstructorCall(ITypeManager typeManager, Expression services, TypeDescriptor knownType,
            Expression[] argsArray)
        {
            var genericMethod = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.StrictConstructorCall));

            var method = genericMethod.MakeGenericMethod(knownType.ImplementingClass);
            
            var arrayOfArgs = Expression.NewArrayInit(typeof(BslValue), argsArray.Select(ConvertToBslValue));
            
            return Expression.Call(method, 
                Expression.Constant(typeManager),
                services,
                Expression.Constant(knownType.Name),
                arrayOfArgs);
        }

        public static Expression TypeByNameCall(ITypeManager manager, Expression argument)
        {
            var method = OperationsCache.GetOrAdd(typeof(DynamicOperations), nameof(DynamicOperations.GetTypeByName),
                BindingFlags.Instance | BindingFlags.Public);
            
            Debug.Assert(method != null);

            return Expression.Call(method, Expression.Constant(manager), argument);
        }

        public static Expression GetExceptionInfo(ParameterExpression excVariable)
        {
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.GetExceptionInfo));

            return Expression.Call(method, excVariable);
        }

        public static Expression AccessModuleVariable(ParameterExpression thisArg, int variableIndex)
        {
            var stateProperty = PropertiesCache.GetOrAdd(
                typeof(NativeClassInstanceWrapper),
                nameof(NativeClassInstanceWrapper.State),
                BindingFlags.Instance | BindingFlags.Public);
            
            var propertyAccess = Expression.Property(thisArg, stateProperty);
            var iVariable = Expression.ArrayIndex(propertyAccess, Expression.Constant(variableIndex));
            var valueProperty = PropertiesCache.GetOrAdd(
                typeof(IValueReference),
                nameof(IValueReference.Value),
                BindingFlags.Instance | BindingFlags.Public);
            
            return Expression.Property(iVariable, valueProperty);
        }

        public static Expression GetContextPropertyValue(IRuntimeContextInstance target, int propertyNumber)
        {
            var getter = OperationsCache.GetOrAdd(
                typeof(IRuntimeContextInstance),
                nameof(IRuntimeContextInstance.GetPropValue),
                BindingFlags.Instance | BindingFlags.Public);

            return Expression.Call(
                Expression.Constant(target),
                getter,
                Expression.Constant(propertyNumber)
            );
        }

        public static Expression GetIndexedValue(Expression target, Expression index)
        {
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.GetIndexedValue));

            return Expression.Call(null, method, target, ConvertToBslValue(index));
        }
        
        public static Expression SetIndexedValue(Expression target, Expression index, Expression value)
        {
            var method = OperationsCache.GetOrAdd(
                typeof(DynamicOperations),
                nameof(DynamicOperations.SetIndexedValue));

            return Expression.Call(null, method, target, ConvertToBslValue(index), ConvertToBslValue(value));
        }
        
        public static Expression InvokeBslNativeMethod(BslNativeMethodInfo nativeMethod, object target, List<Expression> args)
        {
            var helperMethod = OperationsCache.GetOrAdd(
                typeof(CallableMethod),
                nameof(CallableMethod.Invoke),
                BindingFlags.Instance | BindingFlags.Public
            );

            return Expression.Call(
                Expression.Constant(nativeMethod.GetCallable()),
                helperMethod,
                nativeMethod.IsInstance ?
                    InvocationTargetExpression(target) :
                    Expression.Constant(null, typeof(object)),
                PackArgsToArgsArray(args));
        }

        private static Expression PackArgsToArgsArray(List<Expression> args)
        {
            return Expression.NewArrayInit(typeof(BslValue), args.Select(ConvertToBslValue));
        }

        private static Expression[] PrepareInstanceCallArguments(object target, List<Expression> args)
        {
            var actualArgs = new Expression[args.Count + 1];
            actualArgs[0] = InvocationTargetExpression(target);
            for (int i = 0; i < args.Count; i++)
            {
                actualArgs[i + 1] = args[i];
            }

            return actualArgs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Expression InvocationTargetExpression(object target) =>
            target as Expression ?? Expression.Constant(target, typeof(object));
        
    }
}