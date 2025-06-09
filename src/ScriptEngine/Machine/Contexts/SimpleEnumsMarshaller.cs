/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Помогает конвертировать обычные clr-перечисления в значения ClrEnumValueWrapper<T>
    /// Предварительно перечисление должно быть обработано загрузчиком ContextDiscoverer.
    /// </summary>
    public static class SimpleEnumsMarshaller
    {
        // TODO Наверное можно и прямо отсюда регистрировать SimpleEnum-ы а из ContextDiscoverer этот класс вызывать

        private static LruCache<Type, Func<object, EnumerationValue>> _gettersCache 
            = new LruCache<Type, Func<object, EnumerationValue>>(32);
        
        /// <summary>
        /// Получить IValue для значения clr-перечисления.
        /// </summary>
        /// <param name="objParam">Значение перечисления</param>
        /// <param name="type">тип перечисления</param>
        /// <returns>EnumerationValue оборачивающее значение перечисления</returns>
        /// <exception cref="ValueMarshallingException">Что-то пошло не так</exception>
        public static IValue ConvertEnum(object objParam, Type type)
        {
            if (!type.IsInstanceOfType(objParam))
                throw ValueMarshallingException.InvalidEnum(type);

            var getter = _gettersCache.GetOrAdd(type, CreateGetter);

            return getter(objParam);
        }

        private static Func<object, EnumerationValue> CreateGetter(Type type)
        {
            var enumWrapperType = typeof(ClrEnumWrapper<>).MakeGenericType(type);
            var instanceProp = enumWrapperType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            var fromValueMethod =
                enumWrapperType.GetMethod("FromNativeValue", BindingFlags.Instance | BindingFlags.Public);
            
            Debug.Assert(instanceProp != null && fromValueMethod != null);

            var incomingObjectParam = Expression.Parameter(typeof(object), "obj");
            var castedEnum = Expression.Convert(incomingObjectParam, type);
            var propAccess = Expression.Property(null, instanceProp);
            var call = Expression.Call(propAccess, fromValueMethod, castedEnum);

            var lambda = Expression.Lambda<Func<object, EnumerationValue>>(call, incomingObjectParam);
            var getter = lambda.Compile();
            return getter;
        }
    }
}