/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;

namespace OneScript.Native.Compiler
{
    public class ReflectedPropertiesCache : ReflectedMembersCache<PropertyInfo>
    {
        public ReflectedPropertiesCache() : base()
        {
        }

        public ReflectedPropertiesCache(int size): base(size)
        {
        }

        public override PropertyInfo GetOrAdd(Type type, string name)
        {
            return GetOrAdd(type, name, BindingFlags.Public | BindingFlags.Instance);
        }

        protected override PropertyInfo SearchImpl(Type type, string name, BindingFlags flags)
        {
            return type.GetProperty(name, flags);
        }
    }
}