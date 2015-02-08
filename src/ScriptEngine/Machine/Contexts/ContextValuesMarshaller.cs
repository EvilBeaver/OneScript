using System;

namespace ScriptEngine.Machine.Contexts
{
    public static class ContextValuesMarshaller
    {
        public static T ConvertParam<T>(IValue value)
        {
            object valueObj;
            var type = typeof(T);
            if (value == null)
            {
                valueObj = default(T);
            }
            else if (type == typeof(IValue))
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
            else if (type == typeof(int))
            {
                valueObj = (int)value.AsNumber();
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
                valueObj = default(T);
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

        public static IValue ConvertReturnValue<TRet>(TRet param)
        {
            var type = typeof(TRet);
            object objParam = (object)param;
            if (type == typeof(IValue))
            {
                return (IValue)param;
            }
            else if (type == typeof(string))
            {
                return ValueFactory.Create((string)objParam);
            }
            else if (type == typeof(int))
            {
                return ValueFactory.Create((int)objParam);
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
			case Machine.DataType.Object:
				result = val.AsObject();
				if (result is IObjectWrapper)
					result = ((IObjectWrapper)result).UnderlyingObject;
				break;
			default:
				throw new RuntimeException("Тип не поддерживает преобразование в CLR-объект");
			}
			
			return result;
		}
    }
}
