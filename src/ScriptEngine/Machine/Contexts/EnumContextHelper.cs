﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public static class EnumContextHelper
    {
        public static void RegisterValues<T>(T instance) where T : EnumerationContext
        {
            var enumType = typeof(T);
            var values = enumType.GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EnumValueAttribute), false).Any())
                .Select(x => (EnumValueAttribute)x.GetCustomAttributes(typeof(EnumValueAttribute), false)[0]);

            foreach (var enumProperty in values)
            {
                instance.AddValue(enumProperty.GetName(), enumProperty.GetAlias(), new SelfAwareEnumValue<T>(instance));
            }
        }

        public static void RegisterEnumType<T>(out TypeDescriptor enumType, out TypeDescriptor enumValueType) where T : EnumerationContext
        {
            var enumClassType = typeof(T);
            var attribs = enumClassType.GetCustomAttributes(typeof(SystemEnumAttribute), false);

            if (attribs.Length == 0)
                throw new InvalidOperationException("Enum is not marked as SystemEnum");

            var enumMetadata = (SystemEnumAttribute)attribs[0];

            enumType = TypeManager.RegisterType("Перечисление" + enumMetadata.GetName(), typeof(T));
            enumValueType = TypeManager.RegisterType(enumMetadata.GetName(), typeof(SelfAwareEnumValue<T>));
        }

        public static T CreateEnumInstance<T>(EnumCreationDelegate<T> creator) where T : EnumerationContext
        {
            T instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterEnumType<T>(out enumType, out enumValType);

            instance = creator(enumType, enumValType);

            EnumContextHelper.RegisterValues<T>(instance);

            return instance;
        }

    }

    public delegate T EnumCreationDelegate<T>(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) where T : EnumerationContext;

}
