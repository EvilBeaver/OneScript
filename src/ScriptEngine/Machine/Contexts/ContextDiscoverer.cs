using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    static class ContextDiscoverer
    {
        public static void DiscoverClasses(System.Reflection.Assembly assembly)
        {
            var collection = GetMarkedTypes(assembly.GetTypes().AsParallel(), typeof(ContextClassAttribute));

            foreach (var type in collection)
            {
                RegisterSystemType(type);
            }
        }

        public static void DiscoverGlobalContexts(RuntimeEnvironment environment, System.Reflection.Assembly assembly)
        {
            var enums = GetMarkedTypes(assembly.GetTypes().AsParallel(), typeof(SystemEnumAttribute));
            foreach (var item in enums)
            {
                RegisterSystemEnum(item, environment);
            }
        }

        private static IEnumerable<Type> GetMarkedTypes(IEnumerable<Type> allTypes, Type attribute)
        {
            return allTypes.Where(t => t.IsDefined(attribute, false));
        }

        private static void RegisterSystemType(Type stdClass)
        {
            var attribData = stdClass.GetCustomAttributes(typeof(ContextClassAttribute), false);
            System.Diagnostics.Debug.Assert(attribData.Length > 0, "Class is not marked as context");

            var attr = (ContextClassAttribute)attribData[0];
            TypeManager.RegisterType(attr.GetName(), stdClass);
            string alias = attr.GetAlias();
            if(!String.IsNullOrEmpty(alias))
                TypeManager.RegisterType(alias, stdClass);

        }

        private static void RegisterSystemEnum(Type enumType, RuntimeEnvironment environment)
        {
            var method = enumType.GetMethod("GetInstance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            System.Diagnostics.Debug.Assert(method != null, "System enum must have a static method GetInstance");

            var instance = (IValue)method.Invoke(null, null);
            var enumMetadata = (SystemEnumAttribute)enumType.GetCustomAttributes(typeof(SystemEnumAttribute), false)[0];
            environment.InjectGlobalProperty(instance, enumMetadata.GetName(), true);
            if(enumMetadata.GetAlias() != String.Empty)
                environment.InjectGlobalProperty(instance, enumMetadata.GetAlias(), true);
        }

    }
}
