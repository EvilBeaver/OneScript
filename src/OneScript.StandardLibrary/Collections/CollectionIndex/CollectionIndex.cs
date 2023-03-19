/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Localization;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.CollectionIndex
{
    [ContextClass("ИндексКоллекции", "CollectionIndex", TypeUUID = "48D150D4-A0DA-47CA-AEA3-D4078A731C11")]
    public class CollectionIndex : AutoCollectionContext<CollectionIndex, IValue>
    {
        private static readonly TypeDescriptor _objectType = typeof(ValueTableColumnCollection).GetTypeFromClassMarkup();

        private readonly IIndexSourceCollection _source;
        private readonly List<IValue> _fields;

        private readonly IDictionary<CollectionIndexKey, IList<IValue>> _data =
            new Dictionary<CollectionIndexKey, IList<IValue>>();

        public CollectionIndex(IIndexSourceCollection source, IEnumerable<IValue> fields) : base(_objectType)
        {
            _source = source;
            _fields = new List<IValue>(fields);
        }

        internal void ColumnRemoved(IValue column)
        {
            if (_fields.Contains(column))
            {
                _fields.RemoveAll((_c => _c.Equals(column)));
                Rebuild();
            }
        }

        public void AddElement(PropertyNameIndexAccessor element)
        {
            var key = CollectionIndexKey.extract(_fields, element);
            if (_data.ContainsKey(key))
            {
                _data[key].Add(element);
            }
            else
            {
                var list = new List<IValue> { element };
                _data.Add(key, list);
            }
        }

        public void RemoveElement(PropertyNameIndexAccessor element)
        {
            var key = CollectionIndexKey.extract(_fields, element);
            if (_data.ContainsKey(key))
            {
                _data[key].Remove(element);
            }
        }

        private void Rebuild()
        {
            var add = new List<IValue>();
            var delete = new Dictionary<IValue, CollectionIndexKey>();
            foreach (var el in _data)
            {
                foreach (var row in el.Value)
                {
                    var key = CollectionIndexKey.extract(_fields, row as PropertyNameIndexAccessor);
                    if (!key.Equals(el.Key))
                    {
                        delete.Add(row, el.Key);
                        add.Add(row);
                    }
                }
            }

            foreach (var el in delete)
            {
                _data[el.Value].Remove(el.Key);
            }

            foreach (var el in add)
            {
                AddElement(el as PropertyNameIndexAccessor);
            }
        }

        internal void Clear()
        {
            foreach (var el in _data)
            {
                el.Value.Clear();
            }
            _data.Clear();
        }

        /// <summary>
        /// Возвращает покрытие индекса по полям
        /// </summary>
        /// <param name="fields">Поля, по которым будет поиск</param>
        /// <returns> -1 - Индекс не подходит для поиска,
        /// >= 0 - поля индекса входят в поля поиска. Возвращает количество полей поиска, не вошедших в индекс 
        /// </returns>
        public int coverage(IList<IValue> fields)
        {
            var thisKeys = new HashSet<IValue>(_fields);
            var requestedKeys = new HashSet<IValue>(fields);

            if (!requestedKeys.IsSupersetOf(thisKeys))
            {
                return -1;
            }
            requestedKeys.ExceptWith(thisKeys);
            return requestedKeys.Count;
        }

        public IList<IValue> getValues(CollectionIndexKey key)
        {
            var subKey = key.SubKey(_fields);
            return !_data.ContainsKey(subKey) ? new List<IValue>() : _data[subKey];
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var empty = true;
            foreach (var field in _fields)
            {
                if (!empty)
                    builder.Append(", ");
                builder.Append(_source.GetName(field));
                empty = false;
            }

            return builder.ToString();
        }

        public override bool IsIndexed => true;

        public string GetIndexedValue(int index)
        {
            if (index >= 0 && index < _fields.Count)
            {
                return _source.GetName(_fields[index]);
            }
            throw RuntimeException.InvalidArgumentValue();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            var rawIndex = index.GetRawValue();
            if (rawIndex.SystemType == BasicTypes.Number)
            {
                var intIndex = decimal.ToInt32(rawIndex.AsNumber());
                return ValueFactory.Create(GetIndexedValue(intIndex));
            }

            throw RuntimeException.InvalidArgumentType(nameof(index));
        }

        public void SetIndexedValue(int index, string value)
        {
            throw new RuntimeException(new BilingualString("Элемент доступен только для чтения"));
        }

        public override int Count()
        {
            return _fields.Count;
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            foreach (var field in _fields)
            {
                yield return ValueFactory.Create(_source.GetName(field));
            }
        }
    }
}
