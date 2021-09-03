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
using OneScript.Types;
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

        public CollectionIndexes() : base(_instanceType)
        {
        }
        
        [ContextMethod("Добавить", "Add")]
        public CollectionIndex Add(string columns)
        {
            CollectionIndex newIndex = new CollectionIndex();
            _indexes.Add(newIndex);
            return newIndex;
        }

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _indexes.Count();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue Index)
        {
            Index = Index.GetRawValue();
            if (Index is CollectionIndex)
                _indexes.Remove(Index as CollectionIndex);
            else
                _indexes.RemoveAt(Decimal.ToInt32(Index.AsNumber()));
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _indexes.Clear();
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
