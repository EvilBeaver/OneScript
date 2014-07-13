using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    static class ContextValuesMarshaller
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
            else if (type == typeof(double))
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
            else if (type.IsAssignableFrom(typeof(IRuntimeContextInstance)))
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

    }
}
