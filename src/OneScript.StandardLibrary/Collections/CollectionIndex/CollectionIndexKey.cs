/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.StandardLibrary.Collections.ValueTable;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.CollectionIndex
{
    public class CollectionIndexKey
    {
        private readonly IDictionary<IValue, IValue> _values;

        internal CollectionIndexKey(IDictionary<IValue, IValue> values)
        {
            _values = new Dictionary<IValue, IValue>(values);
        }

        /// <summary>
        /// Формирует ключ из источника данных по набору полей.
        /// </summary>
        /// <param name="fields">Поля ключа</param>
        /// <param name="element">Источник значений ключа</param>
        /// <returns></returns>
        public static CollectionIndexKey extract(IList<IValue> fields, PropertyNameIndexAccessor element)
        {
            var values = new Dictionary<IValue, IValue>();
            foreach (var field in fields)
            {
                if (!values.ContainsKey(field))
                {
                    var value = element.GetIndexedValue(field);
                    values.Add(field, value);
                }
            }

            return new CollectionIndexKey(values);
        }

        public static CollectionIndexKey fromKeyAndValueCollection(IEnumerator<KeyAndValueImpl> iterable, IIndexSourceCollection source)
        {
            var values = new Dictionary<IValue, IValue>();
            while (iterable.MoveNext())
            {
                var field = source.GetField(iterable.Current.Key.AsString());
                values.Add(field, iterable.Current.Value);
            }
            return new CollectionIndexKey(values);
        }

        public CollectionIndexKey SubKey(IList<IValue> fields)
        {
            var values = new Dictionary<IValue, IValue>();
            foreach (var field in fields)
            {
                values.Add(field, _values[field]);
            }

            return new CollectionIndexKey(values);
        }

        public IList<IValue> Fields => new List<IValue>(_values.Keys);

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            return Equals(obj as CollectionIndexKey);
        }

        public override int GetHashCode()
        {
            var result = new int[_values.Count];
            int i = 0;
            foreach (var v in _values)
            {
                result[i++] = v.GetHashCode();
            }

            return result.Sum();
        }

        public bool Equals(CollectionIndexKey otherKey)
        {
            if (otherKey == null)
            {
                return false;
            }
            var allKeys = CombineKeys(_values, otherKey._values);
            foreach (var key in allKeys)
            {
                if (!ValuesEqual(key, _values, otherKey._values))
                {
                    return false;
                }
            }

            return true;
        }
        
        private static bool ValuesEqual<T, TX>(T key, IDictionary<T, TX> d1, IDictionary<T, TX> d2)
        {
            if (d1.ContainsKey(key) && d2.ContainsKey(key))
            {
                var v1 = d1[key];
                var v2 = d2[key];
                if (v1.Equals(v2))
                    return true;
            }

            return false;
        }

        private static ISet<T> CombineKeys<T, TX, TY>(IDictionary<T, TX> d1, IDictionary<T, TY> d2)
        {
            var result = new HashSet<T>(d1.Keys);
            foreach (var key in d2.Keys)
            {
                if (!result.Contains(key))
                    result.Add(key);
            }
            return result;
        }
    }
}