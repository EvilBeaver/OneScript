/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEngine.HostedScript.Library.Zip
{
    internal static class ZipCodepageAdHoc
    {
        public static string GetBackingFieldName(string propertyName)
        {
            return $"<{propertyName}>k__BackingField";
        }

        public static FieldInfo GetBackingField(Type type, string propertyName)
        {
            return type.GetField(GetBackingFieldName(propertyName), BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
