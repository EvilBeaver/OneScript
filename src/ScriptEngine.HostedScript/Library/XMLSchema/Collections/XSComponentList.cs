using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("СписокКомпонентXS", "XSComponentList")]
    public class XSComponentList : AutoContext<XSComponentList>, ICollectionContext, IEnumerable<IValue>
    {
        private readonly IXSListOwner _owner;
        private readonly List<IValue> _items;
      
        internal XSComponentList(IXSListOwner owner)
        {
            _items = new List<IValue>();
            _owner = owner;
        }

        #region OneScript

        #region Methods

        [ContextMethod("Вставить", "Insert")]
        public void Insert(int index, IValue value)
        {
            Contract.Requires(value is IXSComponent);

            if (_items.Contains(value))
                return;

            _owner.OnListInsert(this, (IXSComponent)value);
            _items.Insert(index, value);
        }

        [ContextMethod("Добавить", "Add")]
        public void Add(IValue value)
        {
            Contract.Requires(value is IXSComponent);

            if (_items.Contains(value))
                return;

            _owner.OnListInsert(this, (IXSComponent)value);
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
        public IValue Get(int index) => _items[index];
     
        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IValue value) => _items.Contains(value);

        [ContextMethod("Удалить", "Delete")]
        public void Delete(int index)
        {
            IValue value = _items[index];

            _owner.OnListDelete(this, (IXSComponent)value);
            _items.RemoveAt(index);
        }

        public void Delete(IValue value)
        {
            Contract.Requires(value is IXSComponent);

            int index = _items.IndexOf(value);
            if (index == -1)
                return;

            _owner.OnListDelete(this, (IXSComponent)value);
            _items.RemoveAt(index);
        }

        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            Contract.Requires(value is IXSComponent);

            if (_items.Contains(value))
                return;

            IValue currentValue = _items[index];

            _owner.OnListDelete(this, (IXSComponent)currentValue);
            _owner.OnListInsert(this, (IXSComponent)value);

            _items[index] = value;
        }

        #endregion

        #endregion

        #region ICollectionContext

        public CollectionEnumerator GetManagedIterator() => new CollectionEnumerator(GetEnumerator());

        #endregion

        #region IEnumerable

        public IEnumerator<IValue> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
