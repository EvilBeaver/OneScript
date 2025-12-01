/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Contexts.Enums;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Обертка для штатных перечислений Clr, используемых в языке 
    /// </summary>
    /// <typeparam name="T">Оборачиваемое перечисление</typeparam>
    public class ClrEnumWrapper<T> : EnumerationContext where T : struct
    {
        public static ClrEnumWrapper<T> Instance { get; private set; }

        /// <summary>
        /// Constructor for inherited enum wrappers
        /// </summary>
        /// <param name="typeRepresentation"></param>
        /// <param name="valuesType"></param>
        protected ClrEnumWrapper(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) 
            : base(typeRepresentation, valuesType)
        {
        }

        public virtual ClrEnumValueWrapper<T> FromNativeValue(T native)
        {
            var result = TryGetFromNativeValue(native);
            if (result == null)
                throw new InvalidOperationException($"Item '{native}' not found");

            return result;
        }

        public ClrEnumValueWrapper<T> TryGetFromNativeValue(T native)
        {
            ClrEnumValueWrapper<T> wrapper = null;
            foreach (var value in ValuesInternal)
            {
                wrapper = (ClrEnumValueWrapper<T>) value;
                if (wrapper.UnderlyingValue.Equals(native))
                    return wrapper;
            }

            return wrapper;
        }

        private void Autoregister(TypeDescriptor valuesType)
        {
            var attrib = typeof(T).GetCustomAttributes(typeof(EnumerationTypeAttribute), false);
            if(attrib.Length == 0)
                throw new InvalidOperationException($"Enum cannot be autoregistered, no attribute {nameof(EnumerationTypeAttribute)} found");

            var enumType = typeof(T);
            
            foreach (var field in enumType.GetFields())
            {
                foreach (var contextFieldAttribute in field.GetCustomAttributes (typeof (EnumValueAttribute), false))
                {
                    var contextField = (EnumValueAttribute)contextFieldAttribute;

                    string alias = contextField.Alias;
                    if ( alias == null)
                    {
                        if(StringComparer
                            .InvariantCultureIgnoreCase
                            .Compare(field.Name, contextField.Name) != 0)
                            alias = field.Name;
                    }

                    var enumValue = (T)field.GetValue(null)!;
                    var osValue = new ClrEnumValueWrapper<T>(valuesType, enumValue,
                        contextField.Name, alias);
                    
                    AddValue(osValue);
                    
                    // Deprecations
                    foreach (var deprecation in field.GetCustomAttributes<DeprecatedNameAttribute>())
                    {
                        if (deprecation.Name.Equals(contextField.Name, StringComparison.InvariantCultureIgnoreCase)
                            || deprecation.Name.Equals(contextField.Alias, StringComparison.InvariantCultureIgnoreCase))
                        {
                            throw new InvalidOperationException($"Enum value '{contextField.Name}' has same name in deprecations");
                        }
                        
                        var deprecatedValue = new ClrEnumValueWrapper<T>(valuesType, enumValue, deprecation.Name, null);
                        var propertyDef = new SystemPropertyInfo.Builder(deprecation.Name)
                            .SetPropertyType(deprecatedValue.UnderlyingValue.GetType())
                            .SetDeprecated(true)
                            .SetDeclaringType(enumType)
                            .Build();
                        
                        AddValue(deprecatedValue, propertyDef);
                    }
                }
            }
        }
        
        public static ClrEnumWrapper<T> CreateInstance(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
        {
            var instance = new ClrEnumWrapper<T>(typeRepresentation, valuesType);
            instance.Autoregister(valuesType);
            Instance = instance;

            return instance;
        }

        protected static void OnInstanceCreation(ClrEnumWrapper<T> instance)
        {
            Instance = instance;
        }
 
        protected static TE CreateInstance<TE>(ITypeManager typeManager,EnumCreationDelegate<TE> creator)
            where TE: ClrEnumWrapper<T>
        {
           var instance = EnumContextHelper.CreateClrEnumInstance<TE, T>(typeManager, creator);
 
           OnInstanceCreation(instance);
           return instance;
        }
    }

    public abstract class ClrEnumWrapperCached<T> : ClrEnumWrapper<T> where T : struct
    {
        private static readonly Dictionary<T, ClrEnumValueWrapper<T>> _valuesCache
            = new Dictionary<T, ClrEnumValueWrapper<T>>();

        protected ClrEnumWrapperCached(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) 
            : base(typeRepresentation, valuesType)
        {
            _valuesCache.Clear();
        }
        
        protected void MakeValue(string name, string alias, T enumValue)
        {
            _valuesCache[enumValue] = this.WrapClrValue(name, alias, enumValue);
        }
 
        public static new ClrEnumValueWrapper<T> FromNativeValue(T native)
        {
            _valuesCache.TryGetValue(native, out ClrEnumValueWrapper<T> value);
            return value; // TODO: исключение или null?
        }
   }
}