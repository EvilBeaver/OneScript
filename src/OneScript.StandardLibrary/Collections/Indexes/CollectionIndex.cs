/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.Indexes
{
    [ContextClass("ИндексКоллекции", "CollectionIndex")]
    public class CollectionIndex : AutoCollectionContext<CollectionIndex, IValue>
    {
        private readonly List<IValue> _fields = new List<IValue>();
        private readonly IIndexCollectionSource _source;

        private readonly Dictionary<CollectionIndexKey, HashSet<IValue>> _data =
            new Dictionary<CollectionIndexKey, HashSet<IValue>>();
        
        public CollectionIndex(IIndexCollectionSource source, IEnumerable<IValue> fields)
        {
            foreach (var field in fields)
            {
                if (field is ValueTable.ValueTableColumn column) 
                    column.AddToIndex();
                _fields.Add(field);
            }
        
            _source = source;
            foreach (var value in _source)
            {
                ElementAdded(value);
            }
        }

        internal bool CanBeUsedFor(IEnumerable<IValue> searchFields)
        {
            return _fields.Count != 0 && _fields.All(f => searchFields.Contains(f));
        }

        private CollectionIndexKey IndexKey(PropertyNameIndexAccessor source)
        {
            return CollectionIndexKey.Extract(source, _fields);
        }

        public override string ToString()
        {
            return string.Join(", ", _fields.Select(field => _source.GetName(field)));
        }

        public IEnumerable<IValue> GetData(PropertyNameIndexAccessor searchCriteria)
        {
            var key = IndexKey(searchCriteria);
            return _data.TryGetValue(key, out var filteredData) ? filteredData : Enumerable.Empty<IValue>();
        }

        internal void FieldRemoved(IValue field)
        {
            if (_fields.Contains(field))
            {
                while (_fields.Contains(field))
                {
                    if (field is ValueTable.ValueTableColumn column)
                        column.DeleteFromIndex();

                    _fields.Remove(field);
                }
                Rebuild();
            }
        }

        internal void ExcludeFields()
        {
            foreach (var field in _fields)
            {
                if (field is ValueTable.ValueTableColumn column)
                    column.DeleteFromIndex();
            }
        }

        internal void ElementAdded(PropertyNameIndexAccessor element)
        {
            var key = CollectionIndexKey.Extract(element, _fields);
            if (_data.TryGetValue(key, out var set))
            {
                set.Add(element);
            }
            else
            {
                _data.Add(key, new HashSet<IValue> { element });
            }
        }

        internal void ElementRemoved(PropertyNameIndexAccessor element)
        {
            var key = CollectionIndexKey.Extract(element, _fields);
            if (_data.TryGetValue(key, out var set))
            {
                set.Remove(element);
            }
        }

        internal void Clear() => _data.Clear();

        internal void Rebuild()
        {
            _data.Clear();
            foreach (var value in _source)
            {
                ElementAdded(value);
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (index is BslNumericValue numericValue)
            {
                var numeric = numericValue.AsNumber();
                if (numeric >= 0 && numeric < _fields.Count)
                {
                    
                    return ValueFactory.Create(_source.GetName(_fields[decimal.ToInt32(numeric)]));
                }
            }
            throw RuntimeException.InvalidArgumentValue();
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
