/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.Indexes
{
    public class CollectionIndexKey
    {
        private readonly IDictionary<IValue, IValue> _values;
        private readonly int _hashCode;

        private CollectionIndexKey(IDictionary<IValue, IValue> values)
        {
            _values = values;
            _hashCode = EvalHashCode(values.Values);
        }

        private static int EvalHashCode(IEnumerable<IValue> values)
        {
            var result = 0;
            foreach (var value in values)
            {
                result ^= value.GetHashCode();
            }

            return result;
        }

        public static CollectionIndexKey Extract(PropertyNameIndexAccessor source, IEnumerable<IValue> fields)
        {
            var values = new Dictionary<IValue, IValue>();

            foreach (var field in fields)
            {
                var value = source.GetIndexedValue(field);
                values.TryAdd(field, value);
            }

            return new CollectionIndexKey(values);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj is CollectionIndexKey casted)
            {
                var allKeys = CombinedKeysSet(casted);

                foreach (var key in allKeys)
                {
                    var thisValue = _values[key];
                    var otherValue = _values[key];

                    if (!thisValue.Equals(otherValue)) return false;
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private ISet<IValue> CombinedKeysSet(CollectionIndexKey other)
        {
            var allKeys = new HashSet<IValue>();
            allKeys.UnionWith(_values.Keys.ToHashSet());
            allKeys.UnionWith(other._values.Keys.ToHashSet());
            return allKeys;
        }
    }
}