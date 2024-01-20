/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.Indexes
{
    [ContextClass("ИндексКоллекции", "CollectionIndex", TypeUUID = "48D150D4-A0DA-47CA-AEA3-D4078A731C11")]
    public class CollectionIndex : AutoCollectionContext<CollectionIndex, IValue>
    {
        private static readonly TypeDescriptor _objectType = typeof(CollectionIndex).GetTypeFromClassMarkup();

        private readonly List<IValue> _fields = new List<IValue>();
        private readonly IIndexCollectionSource _source;

        private readonly IDictionary<CollectionIndexKey, IList<IValue>> _data =
            new Dictionary<CollectionIndexKey, IList<IValue>>();
        
        public CollectionIndex(IIndexCollectionSource source, IEnumerable<IValue> fields) : base(_objectType)
        {
            _source = source;
            _fields.AddRange(fields);
        }

        internal bool CanBeUsedFor(IEnumerable<IValue> searchFields)
        {
            return _fields.Any() && _fields.ToHashSet().IsSubsetOf(searchFields.ToHashSet());
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
            return _data.TryGetValue(key, out var filteredData) ? filteredData : new List<IValue>();
        }

        internal void FieldRemoved(IValue field)
        {
            if (_fields.Contains(field))
            {
                while (_fields.Contains(field)) _fields.Remove(field);
                Rebuild();
            }
        }

        internal void ElementAdded(PropertyNameIndexAccessor element)
        {
            var key = CollectionIndexKey.Extract(element, _fields);
            if (_data.TryGetValue(key, out var list))
            {
                list.Add(element);
            }
            else
            {
                _data.Add(key, new List<IValue> { element});
            }
        }

        internal void ElementRemoved(PropertyNameIndexAccessor element)
        {
            var key = CollectionIndexKey.Extract(element, _fields);
            if (_data.TryGetValue(key, out var value))
            {
                value.Remove(element);
            }
        }

        internal void Clear()
        {
            _data.Clear();
        }

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
            var rawValue = index.GetRawValue();
            if (rawValue is BslNumericValue numericValue)
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
