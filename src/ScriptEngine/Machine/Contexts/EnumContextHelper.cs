/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    public static class EnumContextHelper
    {
        private static void RegisterValues<T>(T instance) where T : EnumerationContext
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

        private static void RegisterSelfAwareEnumType<T>(ITypeManager typeManager, out TypeDescriptor enumType, out TypeDescriptor enumValueType) where T : EnumerationContext
        {
            (enumType, enumValueType) = RegisterEnumType<T, SelfAwareEnumValue<T>>(typeManager);
        }
        
        public static (TypeDescriptor, TypeDescriptor) RegisterEnumType<TEnum, TValue>(ITypeManager typeManager) 
            where TEnum : EnumerationContext 
            where TValue : EnumerationValue
        {
            return RegisterEnumType(typeof(TEnum), typeof(TValue), typeManager);
        }
        
        public static (TypeDescriptor, TypeDescriptor) RegisterEnumType(Type enumClass, Type enumValueClass, ITypeManager typeManager)
        {
            var attribs = enumClass.GetCustomAttributes(typeof(SystemEnumAttribute), false);

            if (attribs.Length == 0)
                throw new InvalidOperationException($"Enum {enumClass} is not marked as SystemEnum");

            var enumMetadata = (SystemEnumAttribute)attribs[0];

            return RegisterEnumType(enumClass, enumValueClass, typeManager, enumMetadata);
        }

        public static (TypeDescriptor, TypeDescriptor) RegisterEnumType(
            Type enumClass,
            Type enumValueClass,
            ITypeManager typeManager,
            INameAndAliasProvider enumMetadata)
        {
            var enumType = typeManager.RegisterType(
                "Перечисление" + enumMetadata.Name,
                enumMetadata.Alias != default? "Enum" + enumMetadata.Alias : default,
                enumClass);

            var enumValueType = typeManager.RegisterType(
                enumMetadata.Name,
                enumMetadata.Alias,
                enumValueClass);
            
            return (enumType, enumValueType);
        }

        public static T CreateSelfAwareEnumInstance<T>(ITypeManager typeManager, EnumCreationDelegate<T> creator) where T : EnumerationContext
        {
            T instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterSelfAwareEnumType<T>(typeManager, out enumType, out enumValType);

            instance = creator(enumType, enumValType);

            EnumContextHelper.RegisterValues<T>(instance);

            return instance;
        }
        
        public static TOwner CreateClrEnumInstance<TOwner, TEnum>(ITypeManager typeManager, EnumCreationDelegate<TOwner> creator) 
            where TOwner : EnumerationContext
            where TEnum : struct
        {
            TOwner instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            (enumType, enumValType) = EnumContextHelper.RegisterEnumType<TOwner, ClrEnumValueWrapper<TEnum>>(typeManager);

            instance = creator(enumType, enumValType);
            return instance;
        }
        
        public static ClrEnumValueWrapper<T> WrapClrValue<T>(
            this EnumerationContext owner,
            string name,
            string alias,
            T value)
            where T : struct
        {
            var wrappedValue = new ClrEnumValueWrapper<T>(owner, value); 
            owner.AddValue(name, alias, wrappedValue);
            return wrappedValue;
        }

    }

    public delegate T EnumCreationDelegate<T>(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) where T : EnumerationContext;

}
