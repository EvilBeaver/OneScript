using Microsoft.Extensions.Primitives;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using System.Collections.Generic;

namespace OneScript.StandardLibrary.Http.Web
{
    public static class IDictionaryExtensions
    {
        public static MapImpl ToMap(this IEnumerable<KeyValuePair<string, StringValues>> enumerable)
        {
            var map = new MapImpl();

            foreach (var kv in enumerable)
            {
                var key = BslStringValue.Create(kv.Key);
                var value = BslStringValue.Create(kv.Value);

                map.Insert(key, value);
            }

            return map;
        }

        public static FixedMapImpl ToFixedMap(this IEnumerable<KeyValuePair<string, StringValues>> enumerable)
        {
            var map = ToMap(enumerable);

            return new FixedMapImpl(map);
        }
    }
}
