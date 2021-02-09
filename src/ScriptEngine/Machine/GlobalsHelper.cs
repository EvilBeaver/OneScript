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
    public static class GlobalsHelper
    {
        public static T GetEnum<T>()
        {
            var instance = MachineInstance.Current.Globals;
            if (instance != null)
                return instance.GetInstance<T>();

            return default;
        }

        public static EnumerationContext GetEnum(Type type)
        {
            return (EnumerationContext)MachineInstance.Current.Globals?.GetInstance(type);
        }
    }
}
