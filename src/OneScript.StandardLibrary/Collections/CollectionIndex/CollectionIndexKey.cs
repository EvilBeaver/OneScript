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
    public sealed class CollectionIndexKey
    {
        private readonly IDictionary<IValue, IValue> _values;
        private readonly IValue[] _fields;

        private CollectionIndexKey(IDictionary<IValue, IValue> values)
        {
            _values = new Dictionary<IValue, IValue>(values);
            _fields = _values.Keys.ToArray();
        }

        /// <summary>
        /// Формирует ключ из источника данных по набору полей.
        /// </summary>
        /// <param name="fields">Поля ключа</param>
        /// <param name="element">Источник значений ключа</param>
        /// <returns></returns>
        public static CollectionIndexKey extract(IList<IValue> fields, PropertyNameIndexAccessor element)
        {
            var values = new Dictionary<IValue, IValue>(fields.Count);
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
                // TODO: тут возможно переполнение.
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

            if (_values.Count != otherKey._values.Count) return false;

            foreach (var field in _fields)
            {
                if (_values.ContainsKey(field))
                {
                    if (!otherKey._values.ContainsKey(field)) return false;
                    var v1 = _values[field];
                    var v2 = otherKey._values[field];
                    if (!v1.Equals(v2)) return false;
                }
                else
                {
                    if (otherKey._values.ContainsKey(field)) return false;
                }
            }

            return true;
        }
    }
}