/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

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

        public static object ConvertParam(IValue value, Type type)
        {
            object valueObj;
            if (value == null || value.DataType == DataType.NotAValidValue)
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
            else if (value == SimpleConstantValue.Undefined()) 
            {
                // Если тип параметра не IValue и не IVariable && Неопределено -> null
                valueObj = null;
            }
            else if (type == typeof(string))
            {
                valueObj = value.AsString();
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
                var wrapperType = typeof(CLREnumValueWrapper<>).MakeGenericType(new Type[] { type });
                var constructor = wrapperType.GetConstructor(new Type[] { typeof(EnumerationContext), type, typeof(DataType) });
                var osValue = (EnumerationValue)constructor.Invoke(new object[] { null, objParam, DataType.Enumeration });
                return osValue;
            }
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                return ValueFactory.Create((IRuntimeContextInstance)objParam);
            }
            else
            {
                throw new NotSupportedException("Type is not supported");
            }
        }

        public static IValue ConvertReturnValue<TRet>(TRet param)
        {
            var type = typeof(TRet);

            return ConvertReturnValue(param, type);
        }

		public static object ConvertToCLRObject(IValue val)
		{
			object result;
			if (val == null)
				return val;
			
			switch (val.DataType)
			{
			case Machine.DataType.Boolean:
				result = val.AsBoolean();
				break;
			case Machine.DataType.Date:
				result = val.AsDate();
				break;
			case Machine.DataType.Number:
				result = val.AsNumber();
				break;
			case Machine.DataType.String:
				result = val.AsString();
				break;
			case Machine.DataType.Undefined:
				result = null;
				break;
			default:
                if (val.DataType == DataType.Object)
                    result = val.AsObject();

				result = val.GetRawValue();
				if (result is IObjectWrapper)
					result = ((IObjectWrapper)result).UnderlyingObject;
				else
				    throw new ValueMarshallingException($"Тип {val.GetType()} не поддерживает преобразование в CLR-объект");

                break;
			}
			
			return result;
		}

        public static T CastToCLRObject<T>(IValue val)
        {
            return (T)CastToCLRObject(val);
        }

        public static object CastToCLRObject(IValue val)
        {
            var rawValue = val.GetRawValue();
            object objectRef;
            if (rawValue.DataType == DataType.GenericValue)
            {
                objectRef = rawValue;
            }
            else
            {
                objectRef = ConvertToCLRObject(rawValue);
            }

            return objectRef;

        }
    }
}
