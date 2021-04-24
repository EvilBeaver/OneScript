/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Language;

namespace OneScript.Native.Compiler
{
    public class ReflectedMethodsCache : ReflectedMembersCache<MethodInfo>
    {
        public ReflectedMethodsCache() : base()
        {
        }
        
        public ReflectedMethodsCache(int size) : base(size)
        {
        }

        protected override MethodInfo SearchImpl(Type type, string name, BindingFlags flags)
        {
            return type.GetMethod(name, flags);
        }
    }
}