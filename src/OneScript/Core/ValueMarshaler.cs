using OneScript.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    static class ValueMarshaler
    {
        public static T IValueToCLRType<T>(IValue value)
        {
            var type = typeof(T);
            object retVal = IValueToCLRType(value, type);
            if (retVal != null)
                return (T)retVal;
            else
                return default(T);
        }

        public static object IValueToCLRType(IValue value, Type type)
        {
            object valueObj;

            if (value == null)
            {
                valueObj = null;
            }
            else if (type == typeof(IValue))
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
                valueObj = IValueToCLRObject(value);
            }

            return valueObj;
        }

        public static IValue CLRTypeToIValue(object objParam)
        {
            var type = objParam.GetType();
            
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
            else if (type == typeof(double))
            {
                return ValueFactory.Create((double)objParam);
            }
            else if (type == typeof(DateTime))
            {
                return ValueFactory.Create((DateTime)objParam);
            }
            else if (type == typeof(bool))
            {
                return ValueFactory.Create((bool)objParam);
            }
            else if (typeof(ContextBase).IsAssignableFrom(type))
            {
                if (objParam != null)
                    return (ContextBase)objParam;
                else
                    return ValueFactory.Create();
            }
            else
            {
                throw new EngineException("Не удалось преобразовать тип CLR \"" + type.ToString() + "\" в тип OneScript");
            }
        }

        public static object IValueToCLRObject(IValue val)
        {
            object result;
            if (val == null)
                return val;

            if (val.Type == BasicTypes.Boolean)
                result = val.AsBoolean();
            else if (val.Type == BasicTypes.Date)
                result = val.AsDate();
            else if (val.Type == BasicTypes.Number)
                result = val.AsNumber();
            else if (val.Type == BasicTypes.String)
                result = val.AsString();
            else if (val.Type == BasicTypes.Undefined || val.Type == BasicTypes.Null)
                result = null;
            else if (val.Type == BasicTypes.Type)
                result = ((TypeTypeValue)val).ReferencedType;
            else if (val.Type.IsObject)
                result = val.AsObject();
            else
                //result = val.GetRawValue();
                //if (result is IObjectWrapper)
                //    result = ((IObjectWrapper)result).UnderlyingObject;
                //else
                throw new EngineException("Тип не поддерживает преобразование в CLR-объект");

            return result;
        }

        public static object ToCLRType(this IValue value, Type type)
        {
            return IValueToCLRType(value, type);
        }

    }
}
