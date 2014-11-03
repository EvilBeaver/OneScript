using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Library;

namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("ИндексыКоллекции", "CollectionIndexes")]
    class CollectionIndexes : AutoContext<CollectionIndexes>, ICollectionContext
    {

        List<CollectionIndex> _indexes = new List<CollectionIndex>();

        [ContextMethod("Добавить", "Add")]
        public CollectionIndex Add(string columns)
        {
            CollectionIndex newIndex = new CollectionIndex();
            _indexes.Add(newIndex);
            return newIndex;
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _indexes.Count();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue Index)
        {
            IValue Raw = Index.GetRawValue();
            if (Raw is CollectionIndex)
                _indexes.Remove(Raw as CollectionIndex);
            else
                _indexes.RemoveAt(Decimal.ToInt32(Raw.AsNumber()));
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _indexes.Clear();
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _indexes)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

    }
}
