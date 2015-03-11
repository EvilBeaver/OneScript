using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine.Contexts
{
    static class ContextDiscoverer
    {
        private const string INSTANCE_RETRIEVER_NAME = "CreateInstance";
        
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
            var allTypes = assembly.GetTypes();
            var enums = GetMarkedTypes(allTypes.AsParallel(), typeof(SystemEnumAttribute));
            foreach (var item in enums)
            {
                RegisterSystemEnum(item, environment);
            }

            var contexts = GetMarkedTypes(allTypes.AsParallel(), typeof(GlobalContextAttribute));
            foreach (var item in contexts)
            {
                RegisterGlobalContext(item, environment);
            }
        }

        private static IEnumerable<Type> GetMarkedTypes(IEnumerable<Type> allTypes, Type attribute)
        {
            return allTypes.Where(t => t.IsDefined(attribute, false));
        }

        private static void RegisterSystemType(Type stdClass)
        {
            var attribData = stdClass.GetCustomAttributes(typeof(ContextClassAttribute), false);
            System.Diagnostics.Trace.Assert(attribData.Length > 0, "Class is not marked as context");

            var attr = (ContextClassAttribute)attribData[0];
            var newType = TypeManager.RegisterType(attr.GetName(), stdClass);
            string alias = attr.GetAlias();
            if(!String.IsNullOrEmpty(alias))
                TypeManager.RegisterAliasFor(newType, alias);
        }

        private static void RegisterSystemEnum(Type enumType, RuntimeEnvironment environment)
        {
            var method = enumType.GetMethod(INSTANCE_RETRIEVER_NAME, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            System.Diagnostics.Trace.Assert(method != null, "System enum must have a static method " + INSTANCE_RETRIEVER_NAME);

            var instance = (IValue)method.Invoke(null, null);
            GlobalsManager.RegisterInstance(instance);
            var enumMetadata = (SystemEnumAttribute)enumType.GetCustomAttributes(typeof(SystemEnumAttribute), false)[0];
            environment.InjectGlobalProperty(instance, enumMetadata.GetName(), true);
            if(enumMetadata.GetAlias() != String.Empty)
                environment.InjectGlobalProperty(instance, enumMetadata.GetAlias(), true);
        }


        private static void RegisterGlobalContext(Type contextType, RuntimeEnvironment environment)
        {
            var attribData = (GlobalContextAttribute)contextType.GetCustomAttributes(typeof(GlobalContextAttribute), false)[0];
            if (attribData.ManualRegistration)
                return;

            var method = contextType.GetMethod(INSTANCE_RETRIEVER_NAME, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            System.Diagnostics.Trace.Assert(method != null, "Global context must have a static method " + INSTANCE_RETRIEVER_NAME);
            var instance = (IAttachableContext)method.Invoke(null, null);
            GlobalsManager.RegisterInstance(instance);
            environment.InjectObject(instance, false);

        }

    }
}
