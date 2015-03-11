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
                instance.AddValue(enumProperty.GetName(), new SelfAwareEnumValue<T>(instance));
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

     }
}
