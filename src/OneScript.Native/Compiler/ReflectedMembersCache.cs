/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Contexts.Internal;
using OneScript.Language;

namespace OneScript.Native.Compiler
{
    public abstract class ReflectedMembersCache<T> where T : MemberInfo
    {
        private readonly LruCache<string, T> _cache;

        public ReflectedMembersCache() : this(128)
        {
        }

        public ReflectedMembersCache(int size)
        {
            _cache = new LruCache<string, T>(size);
        }

        public virtual T GetOrAdd(Type type, string name)
        {
            return GetOrAdd(type, name, BindingFlags.Public | BindingFlags.Static);
        }

        public T GetOrAdd(Type type, string name, BindingFlags flags)
        {
            var key = $"{type.Name}.{name}";
            return _cache.GetOrAdd(key, x => SearchImpl(type, name, flags) 
                                             ?? throw new InvalidOperationException($"No method found {key}"));
        }

        protected abstract T SearchImpl(Type type, string name, BindingFlags flags);
    }
}