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
