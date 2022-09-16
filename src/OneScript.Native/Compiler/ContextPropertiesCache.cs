/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Reflection;
using OneScript.Contexts;

namespace OneScript.Native.Compiler
{
    public class ContextPropertiesCache : ReflectedMembersCache<PropertyInfo>
    {
        public ContextPropertiesCache()
        {
        }

        public ContextPropertiesCache(int size): base(size)
        {
        }

        public override PropertyInfo GetOrAdd(Type type, string name)
        {
            return GetOrAdd(type, name, BindingFlags.Public | BindingFlags.Instance);
        }

        protected override PropertyInfo SearchImpl(Type type, string name, BindingFlags flags)
        {
            var props = type.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Public | BindingFlags.Instance,
                (info, criteria) =>
                {
                    var a = info.CustomAttributes.FirstOrDefault(data =>
                        data.AttributeType == typeof(ContextPropertyAttribute));
                    if (a == null) return false;
                    return a.ConstructorArguments.Any(alias =>
                        StringComparer.CurrentCultureIgnoreCase.Equals(alias.Value.ToString(), name));
                },
                null);

            if (props.Length != 1)
            {
                return null;
            }

            return props[0] as PropertyInfo;
        }
    }
}