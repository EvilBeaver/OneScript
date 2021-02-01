/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine
{
    public static class GlobalsManager
    {
        // FIXME: вынужденно writable, т.к. потребители пользуются статическим инстансом
        public static IGlobalsManager Instance { get; set; } = new GlobalInstancesManager();

        internal static void Reset()
        {
            Instance.Dispose();
        }

        public static void RegisterInstance(object instance)
        {
            Instance.RegisterInstance(instance);
        }

        public static T GetGlobalContext<T>()
        {
            return Instance.GetInstance<T>();
        }

        public static T GetEnum<T>()
        {
            return Instance.GetInstance<T>();
        }

        public static EnumerationContext GetSimpleEnum(Type type)
        {
            return (EnumerationContext)Instance.GetInstance(type);
        }
    }
}
