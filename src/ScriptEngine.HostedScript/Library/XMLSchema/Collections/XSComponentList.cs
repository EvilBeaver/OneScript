using System.Collections;
using System.Collections.Generic;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("СписокКомпонентXS", "XSComponentList")]
    public class XSComponentList : AutoContext<XSComponentList>, ICollectionContext, IEnumerable<IXSComponent>
    {
        private readonly IXSListOwner _owner;
        private readonly List<IXSComponent> _items;
      
        internal XSComponentList(IXSListOwner owner)
        {
            _items = new List<IXSComponent>();
            _owner = owner;
        }

        #region OneScript

        #region Methods

        [ContextMethod("Вставить", "Insert")]
        public void Insert(int index, IXSComponent value)
        {
            if (_items.Contains(value))
                return;

            _owner.OnListInsert(this, value);
            _items.Insert(index, value);
        }

        [ContextMethod("Добавить", "Add")]
        public void Add(IXSComponent value)
        {

            if (_items.Contains(value))
                return;

            _owner.OnListInsert(this, value);
            _items.Add(value);
        }

        [ContextMethod("Количество", "Count")]
        public int Count() => _items.Count;

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _owner.OnListClear(this);
            _items.Clear();
        }

        [ContextMethod("Получить", "Get")]
        public IXSComponent Get(int index) => _items[index];
     
        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent value) => _items.Contains(value);

        [ContextMethod("Удалить", "Delete")]
        public void Delete(int index)
        {
            IXSComponent value = _items[index];

            _owner.OnListDelete(this, value);
            _items.RemoveAt(index);
        }

        public void Delete(IXSComponent value)
        {
            int index = _items.IndexOf(value);
            if (index == -1)
                return;

            _owner.OnListDelete(this, value);
            _items.RemoveAt(index);
        }

        [ContextMethod("Установить", "Set")]
        public void Set(int index, IXSComponent value)
        {
            if (_items.Contains(value))
                return;

            IXSComponent currentValue = _items[index];

            _owner.OnListDelete(this, currentValue);
            _owner.OnListInsert(this, value);

            _items[index] = value;
        }

        #endregion

        #endregion

        #region ICollectionContext

        public CollectionEnumerator GetManagedIterator() => new CollectionEnumerator(GetEnumerator());

        #endregion

        #region IEnumerable

        public IEnumerator<IXSComponent> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
