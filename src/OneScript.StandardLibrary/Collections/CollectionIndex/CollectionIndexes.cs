/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.CollectionIndex
{
    [ContextClass("ИндексыКоллекции", "CollectionIndexes", TypeUUID = "75983CBE-2ACC-4925-9CE0-23FC0C3E3211")]
    public class CollectionIndexes : AutoCollectionContext<CollectionIndexes, CollectionIndex>
    {
        private static readonly TypeDescriptor _instanceType = typeof(CollectionIndexes).GetTypeFromClassMarkup();

        private readonly List<CollectionIndex> _indexes = new List<CollectionIndex>();

        private readonly IIndexSourceCollection _source;

        public CollectionIndexes(IIndexSourceCollection source) : base(_instanceType)
        {
            _source = source;
        }
        
        [ContextMethod("Добавить", "Add")]
        public CollectionIndex Add(string columns)
        {
            var fields = _source.GetFields(columns);
            var newIndex = new CollectionIndex(_source, fields);
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
            var indexToDelete = GetIndexInternal(index);
            indexToDelete.Clear();
            _indexes.Remove(indexToDelete);
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            foreach (var collectionIndex in _indexes)
            {
                collectionIndex.Clear();
            }
            _indexes.Clear();
        }

        private CollectionIndex GetIndexInternal(IValue index)
        {
            var rawIndex = index.GetRawValue();
            if (rawIndex is CollectionIndex collectionIndex)
            {
                if (_indexes.Contains(collectionIndex))
                    return collectionIndex;
            } else
            if (rawIndex.SystemType == BasicTypes.Number)
            {
                var intIndex = Decimal.ToInt32(rawIndex.AsNumber());
                if (intIndex >= 0 && intIndex < _indexes.Count)
                    return _indexes[intIndex];
            }
            throw RuntimeException.InvalidArgumentValue();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return GetIndexInternal(index);
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
