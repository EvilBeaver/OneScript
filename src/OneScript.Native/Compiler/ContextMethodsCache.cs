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
using OneScript.Types;

namespace OneScript.Native.Compiler
{
    /// <summary>
    /// L2 Cache for context methods
    /// </summary>
    public class ContextMethodsCache : ReflectedMethodsCache
    {
        private struct CacheRecord
        {
            public string Name;
            public string Alias;
            public MethodInfo Method;
        }

        private const int _l2Size = 1024;
        private Dictionary<Type, CacheRecord[]> _methodCache = new Dictionary<Type, CacheRecord[]>();
        
        public ContextMethodsCache() : base()
        {
        }
        
        public ContextMethodsCache(int size) : base(size)
        {
        }

        public override MethodInfo GetOrAdd(Type type, string name)
        {
            return GetOrAdd(type, name, BindingFlags.Instance | BindingFlags.Public);
        }

        protected override MethodInfo SearchImpl(Type type, string name, BindingFlags flags)
        {
            if(!_methodCache.TryGetValue(type, out var cacheRecords))
            {
                cacheRecords = type.GetMethods()
                    .Select(x => new
                    {
                        Method = x,
                        Markup = x.GetCustomAttribute<ContextMethodAttribute>()
                    })
                    .Where(x => x.Markup != null)
                    .Select(x => new CacheRecord
                    {
                        Name = x.Markup.GetName(),
                        Alias = x.Markup.GetAlias(),
                        Method = x.Method
                    }).ToArray();
                
                if(_methodCache.Count > _l2Size)
                    _methodCache.Clear();
                
                _methodCache.Add(type, cacheRecords);
            }

            return cacheRecords.FirstOrDefault(x => 
                name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) 
                || (x.Alias != default && name.Equals(x.Alias, StringComparison.OrdinalIgnoreCase)))
                .Method;
        }
    }
}