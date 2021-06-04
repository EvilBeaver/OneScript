/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using OneScript.Commons;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public static class ContextValuesMarshaller
    {
        public static T ConvertParam<T>(IValue value)
        {
            var type = typeof(T);
            object valueObj = ConvertParam(value, type);
            if (valueObj == null)
            {
                return default(T);
            }
            
            try
            {
                return (T)valueObj;
            }
            catch (InvalidCastException)
            {
                throw RuntimeException.InvalidArgumentType();
            }
           
        }

        public static T ConvertParam<T>(IValue value, T defaultValue)
        {
            var type = typeof(T);
            object valueObj = ConvertParam(value, type);
            if (valueObj == null)
            {
                return defaultValue;
            }
            
            try
            {
                return (T)valueObj;
            }
            catch (InvalidCastException)
            {
                throw RuntimeException.InvalidArgumentType();
            }
           
        }

        public static object ConvertParam(IValue value, Type type)
        {
            object valueObj;
            if (value == null || value.IsSkippedArgument())
            {
                return null;
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return ConvertParam(value, Nullable.GetUnderlyingType(type));
            }

            if (type == typeof(IValue))
            {
                valueObj = value;
            }
            else if (type == typeof(IVariable))
            {
                valueObj = value;
            }
            else if (type == typeof(string))
            {
                valueObj = value.AsString();
            }
            else if (value == BslUndefinedValue.Instance)
            {
                // Если тип параметра не IValue и не IVariable && Неопределено -> null
                valueObj = null;
            }
            else if (type == typeof(int))
            {
                valueObj = (int)value.AsNumber();
            }
            else if (type == typeof(sbyte))
            {
                valueObj = (sbyte)value.AsNumber();
            }
            else if (type == typeof(short))
            {
                valueObj = (short)value.AsNumber();
            }
            else if (type == typeof(ushort))
            {
                valueObj = (ushort)value.AsNumber();
            }
            else if (type == typeof(uint))
            {
                valueObj = (uint)value.AsNumber();
            }
            else if (type == typeof(byte))
            {
                valueObj = (byte)value.AsNumber();
            }
            else if (type == typeof(long))
            {
                valueObj = (long)value.AsNumber();
            }
            else if (type == typeof(ulong))
            {
                valueObj = (ulong)value.AsNumber();
            }
            else if (type == typeof(double))
            {
                valueObj = (double)value.AsNumber();
            }
            else if (type == typeof(decimal))
            {
                valueObj = value.AsNumber();
            }
            else if (type == typeof(DateTime))
            {
                valueObj = value.AsDate();
            }
            else if (type == typeof(bool))
            {
                valueObj = value.AsBoolean();
            }
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                valueObj = value.AsObject();
            }
            else
            {
                valueObj = CastToCLRObject(value);
            }

            return valueObj;
        }

        public static IValue ConvertReturnValue(object objParam, Type type)
        {
            if (objParam == null)
                return ValueFactory.Create();

            if (type == typeof(IValue))
            {
                return (IValue)objParam;
            }
            else if (type == typeof(string))
            {
                return ValueFactory.Create((string)objParam);
            }
            else if (type == typeof(int))
            {
                return ValueFactory.Create((int)objParam);
            }
            else if (type == typeof(uint))
            {
                return ValueFactory.Create((uint)objParam);
            }
            else if (type == typeof(long))
            {
                return ValueFactory.Create((long)objParam);
            }
            else if (type == typeof(ulong))
            {
                return ValueFactory.Create((ulong)objParam);
            }
            else if (type == typeof(decimal))
            {
                return ValueFactory.Create((decimal)objParam);
            }
            else if (type == typeof(double))
            {
                return ValueFactory.Create((decimal)(double)objParam);
            }
            else if (type == typeof(DateTime))
            {
                return ValueFactory.Create((DateTime)objParam);
            }
            else if (type == typeof(bool))
            {
                return ValueFactory.Create((bool)objParam);
            }
            else if (type.IsEnum)
            {
                return ConvertEnum(objParam, type);
            }
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                return (IValue)(IRuntimeContextInstance)objParam;
            }
            else if (typeof(IValue).IsAssignableFrom(type))
            {
                return (IValue)objParam;
            }
            else
            {
                throw new NotSupportedException($"Type {type} is not supported");
            }
        }

        private static IValue ConvertEnum(object objParam, Type type)
        {
            if (!type.IsAssignableFrom(objParam.GetType()))
                throw new RuntimeException("Некорректный тип конвертируемого перечисления");

            var memberInfo = type.GetMember(objParam.ToString());
            var valueInfo = memberInfo.FirstOrDefault(x => x.DeclaringType == type);
            var attrs = valueInfo.GetCustomAttributes(typeof(EnumItemAttribute), false);

            if (attrs.Length == 0)
                throw new RuntimeException("Значение перечисления должно быть помечено атрибутом EnumItemAttribute");

            var itemName = ((EnumItemAttribute)attrs[0]).Name;
            var enumImpl = GlobalsHelper.GetEnum(type);

            return enumImpl.GetPropValue(itemName);
        }

        public static IValue ConvertReturnValue<TRet>(TRet param)
        {
            var type = typeof(TRet);

            return ConvertReturnValue(param, type);
        }

		public static object ConvertToCLRObject(IValue value)
		{
            if (value == null)
                return null;
            
            var raw = value.GetRawValue();
            switch (raw)
            {
                case BslNumericValue num:
                    return (decimal) num;
                case BslBooleanValue boolean:
                    return (bool) boolean;
                case BslStringValue str:
                    return (string) str;
                case BslDateValue date:
                    return (DateTime) date;
                case BslUndefinedValue _:
                    return null;
                case BslNullValue _:
                    return null;
                case BslTypeValue type:
                    return type.SystemType.ImplementingClass;
                case IObjectWrapper wrapper:
                    return wrapper.UnderlyingObject;
                default:
                    return value;
            }
        }

        public static object CastToCLRObject(IValue val)
        {
            var rawValue = val.GetRawValue();
            object objectRef;
            if (rawValue is IObjectWrapper wrapper)
            {
                objectRef = wrapper.UnderlyingObject;
            }
            else
            {
                objectRef = ConvertToCLRObject(rawValue);
            }

            return objectRef;

        }
    }
}
