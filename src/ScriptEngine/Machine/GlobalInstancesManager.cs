using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public static class GlobalsManager
    {
        static Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        internal static void RegisterInstance(object instance)
        {
            _instances.Add(instance.GetType(), instance);
        }

        public static T GetGlobalContext<T>()
        {
            return InternalGetInstance<T>();
        }

        public static T GetEnum<T>()
        {
            return InternalGetInstance<T>();
        }

        private static T InternalGetInstance<T>()
        {
            return (T)_instances[typeof(T)];
        }
    }
}
