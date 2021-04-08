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
    public class ReflectedMethodsCache
    {
        private LexemTrie<MethodInfo> _cache = new LexemTrie<MethodInfo>();

        public MethodInfo GetOrAdd(Type type, string name)
        {
            return GetOrAdd(type, name, BindingFlags.Public | BindingFlags.Static);
        }
        
        public MethodInfo GetOrAdd(Type type, string name, BindingFlags flags)
        {
            var key = $"{type.Name}.{name}";
            if (!_cache.TryGetValue(key, out var result))
            {
                result = type.GetMethod(name, flags);
                if (result == null)
                    throw new InvalidOperationException($"No method found {key}");
                _cache.Add(key, result);
            }

            return result;
        }
    }
}