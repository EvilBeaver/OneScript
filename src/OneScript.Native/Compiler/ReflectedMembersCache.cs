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
    public abstract class ReflectedMembersCache<T> where T : MemberInfo
    {
        protected LexemTrie<T> _cache = new LexemTrie<T>();
        protected int _cacheSize;

        public ReflectedMembersCache()
        {
            _cacheSize = 128;
        }

        public ReflectedMembersCache(int size)
        {
            _cacheSize = size;
        }

        public virtual T GetOrAdd(Type type, string name)
        {
            return GetOrAdd(type, name, BindingFlags.Public | BindingFlags.Static);
        }

        public T GetOrAdd(Type type, string name, BindingFlags flags)
        {
            var key = $"{type.Name}.{name}";
            if (!_cache.TryGetValue(key, out var result))
            {
                result = SearchImpl(type, name, flags);
                if (result == null)
                    throw new InvalidOperationException($"No method found {key}");

                if (_cache.Count >= _cacheSize)
                    _cache = new LexemTrie<T>();
                
                _cache.Add(key, result);
            }

            return result;
        }

        protected abstract T SearchImpl(Type type, string name, BindingFlags flags);
    }
}