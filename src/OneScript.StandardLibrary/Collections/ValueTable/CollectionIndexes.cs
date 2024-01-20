/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections.Indexes;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    [ContextClass("ИндексыКоллекции", "CollectionIndexes", TypeUUID = "75983CBE-2ACC-4925-9CE0-23FC0C3E3211")]
    public class CollectionIndexes : AutoCollectionContext<CollectionIndexes, CollectionIndex>
    {
        private static readonly TypeDescriptor _instanceType = typeof(CollectionIndexes).GetTypeFromClassMarkup();
        
        readonly List<CollectionIndex> _indexes = new List<CollectionIndex>();
        private readonly IIndexCollectionSource _owner;

        public CollectionIndexes(IIndexCollectionSource owner) : base(_instanceType)
        {
            _owner = owner;
        }
        
        [ContextMethod("Добавить", "Add")]
        public CollectionIndex Add(string columns)
        {
            var newIndex = new CollectionIndex(_owner, BuildFieldList(_owner, columns));
            newIndex.Rebuild();
            _indexes.Add(newIndex);
            return newIndex;
        }

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _indexes.Count();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue index)
        {
            _indexes.Remove(GetIndex(index));
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _indexes.Clear();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return GetIndex(index);
        }

        private CollectionIndex GetIndex(IValue index)
        {
            index = index.GetRawValue();
            if (index is CollectionIndex collectionIndex)
            {
                if (_indexes.Contains(collectionIndex))
                {
                    return collectionIndex;
                }
                throw RuntimeException.InvalidArgumentValue();
            }

            if (index is BslNumericValue numberValue)
            {
                var number = numberValue.AsNumber();
                if (number >= 0 && number < _indexes.Count)
                {
                    return _indexes[decimal.ToInt32(number)];
                }

                throw RuntimeException.InvalidArgumentValue();
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public void Rebuild()
        {
            foreach (var index in _indexes)
            {
                index.Rebuild();
            }
        }

        internal void ClearIndexes()
        {
            foreach (var index in _indexes)
            {
                index.Clear();
            }
        }

        internal void FieldRemoved(IValue field)
        {
            foreach (var index in _indexes)
            {
                index.FieldRemoved(field);
            }
        }

        internal void ElementAdded(PropertyNameIndexAccessor element)
        {
            foreach (var index in _indexes)
            {
                index.ElementAdded(element);
            }
        }

        internal void ElementRemoved(PropertyNameIndexAccessor element)
        {
            foreach (var index in _indexes)
            {
                index.ElementRemoved(element);
            }
        }

        public CollectionIndex FindSuitableIndex(IEnumerable<IValue> searchFields)
        {
            return _indexes.FirstOrDefault(index => index.CanBeUsedFor(searchFields));
        }

        private static IList<IValue> BuildFieldList(IIndexCollectionSource source, string fieldList)
        {
            var fields = new List<IValue>();
            var fieldNames = fieldList.Split(',');
            foreach (var fieldName in fieldNames)
            {
                if (!string.IsNullOrWhiteSpace(fieldName))
                {
                    var field = source.GetField(fieldName.Trim());
                    if (field == null)
                    {
                        throw new ColumnNotFoundException(fieldName);
                    }
                    fields.Add(field);
                }
            }

            return fields;
        }

        public override IEnumerator<CollectionIndex> GetEnumerator()
        {
            foreach (var item in _indexes)
            {
                yield return item;
            }
        }
    }
}
