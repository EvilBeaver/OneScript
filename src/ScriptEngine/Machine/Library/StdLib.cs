using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    static class StdLib
    {
        private static Dictionary<Type, TypeDescriptor> _knownTypes = new Dictionary<Type, TypeDescriptor>();

        public static void Initialize()
        {
            TypeManager.Initialize(new StandartTypeManager());
            _knownTypes.Clear();
            var collection = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().AsParallel()
                .Where(t=>t.IsDefined(typeof(ContextClassAttribute), false));
            foreach (var type in collection)
            {
                RegisterSystemType(type);
            }
        }

        public static bool IsKnownType(Type type)
        {
            return _knownTypes.ContainsKey(type);
        }

        public static bool IsKnownType(Type type, out TypeDescriptor descriptor)
        {
            return _knownTypes.TryGetValue(type, out descriptor);
        }

        private static void RegisterSystemType(Type stdClass)
        {
            var attribData = stdClass.GetCustomAttributes(typeof(ContextClassAttribute), false);
            System.Diagnostics.Debug.Assert(attribData.Length > 0, "Class is not marked as context");

            var attr = (ContextClassAttribute)attribData[0];
            TypeManager.RegisterType(attr.GetName(), stdClass);

            _knownTypes.Add(stdClass, TypeManager.GetTypeByName(attr.GetName()));

        }
    }
}
