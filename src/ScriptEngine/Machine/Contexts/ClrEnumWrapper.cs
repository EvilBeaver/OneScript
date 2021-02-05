/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Types;

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
        /// Internal constructor, used for discovering SimpleEnums only
        /// </summary>
        /// <param name="typeRepresentation"></param>
        /// <param name="valuesType"></param>
        /// <param name="autoRegister"></param>
        /// <exception cref="InvalidOperationException">Attempt to construct second wrapper for enum</exception>
        // ReSharper disable once MemberCanBePrivate.Global
        internal ClrEnumWrapper(TypeDescriptor typeRepresentation, TypeDescriptor valuesType, bool autoRegister) : base(typeRepresentation, valuesType)
        {
            if (Instance != default)
                return;
            
            if(autoRegister)
                Autoregister();

            Instance = this;
        }

        /// <summary>
        /// Constructor for inherited enum wrappers
        /// </summary>
        /// <param name="typeRepresentation"></param>
        /// <param name="valuesType"></param>
        protected ClrEnumWrapper(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) : this(
            typeRepresentation, valuesType, false)
        {
        }

        public virtual ClrEnumValueWrapper<T> FromNativeValue(T native)
        {
            foreach (var value in ValuesInternal)
            {
                var wrapper = (ClrEnumValueWrapper<T>) value;
                if (wrapper.UnderlyingValue.Equals(native))
                    return wrapper;
            }

            throw new InvalidOperationException($"Item '{native}' not found");
        }
        
        private void Autoregister()
        {
            var attrib = typeof(T).GetCustomAttributes(typeof(EnumerationTypeAttribute), false);
            if(attrib.Length == 0)
                throw new InvalidOperationException($"Enum cannot be autoregistered, no attribute {nameof(EnumerationTypeAttribute)} found");

            var enumType = typeof(T);
            
            foreach (var field in enumType.GetFields())
            {
                foreach (var contextFieldAttribute in field.GetCustomAttributes (typeof (EnumItemAttribute), false))
                {
                    var contextField = (EnumItemAttribute)contextFieldAttribute;
                    var osValue = new ClrEnumValueWrapper<T>(this, (T)field.GetValue(null));

                    if (contextField.Alias == null)
                    {
                        if(StringComparer
                            .InvariantCultureIgnoreCase
                            .Compare(field.Name, contextField.Name) != 0)
                            AddValue(contextField.Name, field.Name, osValue);
                        else
                            AddValue(contextField.Name, osValue);
                    }
                    else
                        AddValue(contextField.Name, contextField.Alias, osValue);
                }
            }
        }
    }
}