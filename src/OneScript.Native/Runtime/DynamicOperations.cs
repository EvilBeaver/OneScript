/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Language;
using OneScript.Localization;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Native.Runtime
{
    internal static class DynamicOperations
    {
        public static BslValue Add(BslValue left, BslValue right)
        {
            if (left is BslStringValue str)
                return BslStringValue.Create(str + right);
            
            if (left is BslDateValue bslDate && right is BslNumericValue num)
            {
                return BslDateValue.Create(bslDate - (decimal) num);
            }
            
            var dLeft = (decimal)left;
            var dRight = (decimal)right;
            return BslNumericValue.Create(dLeft + dRight);
        }

        public static BslValue Subtract(BslValue left, BslValue right)
        {
            if (left is BslNumericValue num)
            {
                var result = num - (decimal)right;
                return BslNumericValue.Create(result);
            }
            else if (left is BslDateValue date)
            {
                switch (right)
                {
                    case BslNumericValue numRight:
                    {
                        var result = date - numRight;
                        return BslDateValue.Create(result);
                    }
                    case BslDateValue dateRight:
                    {
                        var result = date - dateRight;
                        return BslNumericValue.Create(result);
                    }
                }
            }
            else
            {
                var dLeft = (decimal)left;
                var dRight = (decimal)right;
                return BslNumericValue.Create(dLeft - dRight);
            }

            throw BslExceptions.ConvertToNumberException();
        }
        
        public static bool ToBoolean(BslValue value)
        {
            return (bool)value;
        }
        
        public static decimal ToNumber(BslValue value)
        {
            return (decimal)value;
        }
        
        public static DateTime ToDate(BslValue value)
        {
            return (DateTime)value;
        }
        
        public static string ToString(BslValue value)
        {
            return (string)value;
        }

        public static BslValue WrapClrObjectToValue(object value)
        {
            return value switch
            {
                null => BslUndefinedValue.Instance,
                string s => BslStringValue.Create(s),
                decimal d => BslNumericValue.Create(d),
                int n => BslNumericValue.Create(n),
                long l => BslNumericValue.Create(l),
                double dbl => BslNumericValue.Create((decimal) dbl),
                bool boolean => BslBooleanValue.Create(boolean),
                DateTime date => BslDateValue.Create(date),
                _ => throw new TypeConversionException(new BilingualString(
                    $"Невозможно преобразовать {value.GetType()} в тип {nameof(BslValue)}",
                    $"Can't Convert {value.GetType()} to {nameof(BslValue)}"))
            };
        }
        
        public static BslValue ConstructorCall(ITypeManager typeManager, IServiceContainer services, string typeName, BslValue[] args)
        {
            var type = typeManager.GetTypeByName(typeName);
            var factory = typeManager.GetFactoryFor(type);
            var context = new TypeActivationContext
            {
                TypeManager = typeManager,
                Services = services,
                TypeName = type.Name
            };
            
            return (BslValue) factory.Activate(context, args.Cast<IValue>().ToArray());
        }
        
        // TODO: Сделать прямой маппинг на статические фабрики-методы, а не через Factory.Activate
        public static T StrictConstructorCall<T>(ITypeManager typeManager, IServiceContainer services, string typeName, BslValue[] args)
            where T : BslValue
        {
            return (T) ConstructorCall(typeManager, services, typeName, args);
        }

        public static ExceptionInfoClass GetExceptionInfo(Exception e)
        {
            if (e is ScriptException s)
                return new ExceptionInfoClass(s);

            var wrapper = new ExternalSystemException(e);

            return new ExceptionInfoClass(wrapper);
        }

        public static TypeDescriptor GetTypeByName(ITypeManager manager, string name)
        {
            var firstTry = manager.GetTypeByName(name);
            
            // костыль подмены типа для Тип("ИнформацияОбОшибке")
            if (firstTry.Name == ExceptionInfoClass.LanguageType.Name)
            {
                return ExceptionInfoClass.LanguageType;
            }

            return firstTry;
        }

        public static BslValue GetIndexedValue(object target, BslValue index)
        {
            if (!(target is IRuntimeContextInstance context) || !context.IsIndexed)
            {
                throw RuntimeException.IndexedAccessIsNotSupportedException();
            }

            return (BslValue)context.GetIndexedValue((IValue)index);
        }
        
        public static void SetIndexedValue(object target, BslValue index, BslValue value)
        {
            if (!(target is IRuntimeContextInstance context) || !context.IsIndexed)
            {
                throw RuntimeException.IndexedAccessIsNotSupportedException();
            }

            context.SetIndexedValue(index, value);
        }

        public static BslValue GetPropertyValue(BslValue target, string propertyName)
        {
            if (!(target is IRuntimeContextInstance context))
                throw BslExceptions.ValueIsNotObjectException();

            var propIndex = context.GetPropertyNumber(propertyName);
            return (BslValue)context.GetPropValue(propIndex);
        }
    }
}