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
            else if (type == typeof(string))
            {
                valueObj = value.AsString();
            }
            else if (type == typeof(int) || type == typeof(uint) || type == typeof(short) || type == typeof(ushort) || type == typeof(byte) || type == typeof(sbyte))
            {
                valueObj = (int)value.AsNumber();
            }
            else if (type == typeof(long) || type == typeof(ulong))
            {
                valueObj = (long)value.AsNumber();
            }
            else if (type == typeof(double) || type == typeof(decimal))
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

        public static IValue ConvertReturnValue<TRet>(TRet param)
        {
            var type = typeof(TRet);
            object objParam = (object)param;
            if (type == typeof(IValue))
            {
                if (param != null)
                    return (IValue)param;
                else
                    return ValueFactory.Create();
            }
            else if (type == typeof(string))
            {
                return ValueFactory.Create((string)objParam);
            }
            else if (type == typeof(int))
            {
                return ValueFactory.Create((int)objParam);
            }
            else if (type == typeof(long))
            {
                return ValueFactory.Create((long)objParam);
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
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                if (objParam != null)
                    return ValueFactory.Create((IRuntimeContextInstance)objParam);
                else
                    return ValueFactory.Create();
            }
            else
            {
                throw new NotSupportedException("Type is not supported");
            }

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
				    throw new ValueMarshallingException("Тип не поддерживает преобразование в CLR-объект");

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
