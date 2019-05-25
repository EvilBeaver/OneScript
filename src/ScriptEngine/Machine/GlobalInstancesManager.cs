/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public static class GlobalsManager
    {
        static readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        internal static void Reset()
        {
            foreach (var disposable in _instances
                .Select(x=>x.Value)                   
                .Where(x => x is IDisposable))
            {
                ((IDisposable)disposable).Dispose();
            }
            
            _instances.Clear();
        }

        public static void RegisterInstance(object instance)
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
