using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    static class ContextDiscoverer
    {
        public static void Discover(System.Reflection.Assembly assembly)
        {
            var collection = assembly.GetTypes().AsParallel()
                .Where(t=>t.IsDefined(typeof(ContextClassAttribute), false));
            foreach (var type in collection)
            {
                RegisterSystemType(type);
            }
        }

        private static void RegisterSystemType(Type stdClass)
        {
            var attribData = stdClass.GetCustomAttributes(typeof(ContextClassAttribute), false);
            System.Diagnostics.Debug.Assert(attribData.Length > 0, "Class is not marked as context");

            var attr = (ContextClassAttribute)attribData[0];
            TypeManager.RegisterType(attr.GetName(), stdClass);
            TypeManager.RegisterType(attr.GetAlias(), stdClass);

        }

    }
}
