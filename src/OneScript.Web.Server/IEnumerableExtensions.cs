﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.Extensions.Primitives;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneScript.Web.Server
{
    internal static class IEnumerableExtensions
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
