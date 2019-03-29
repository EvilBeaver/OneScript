using System;
using System.Collections;
using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("СписокКомпонентXS", "XSComponentList")]
    public class XSComponentList : AutoContext<XSComponentList>, ICollectionContext, IEnumerable<IXSComponent>
    {
        private readonly IXSListOwner _owner;
        private readonly List<IXSComponent> _items;

        private void InvokeEventCleared() => Cleared?.Invoke(this, EventArgs.Empty);

        internal XSComponentList(IXSListOwner owner)
        {
            _items = new List<IXSComponent>();
            _owner = owner;
        }

        public event EventHandler Cleared;

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
            _items.Clear();
            InvokeEventCleared();
        }

        [ContextMethod("Получить", "Get")]
        public IXSComponent Get(int index) => _items[index];
     
        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent value) => _items.Contains(value);

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue value)
        {
            int index;

            switch (value.DataType)
            {
                case DataType.Number:
                    index = (int)value.AsNumber();
                    break;

                case DataType.Object:
                    index = _items.IndexOf(value as IXSComponent);
                    if (index == -1)
                        return;
                    break;

                default:
                    throw RuntimeException.InvalidArgumentType();
            }

            _owner.OnListDelete(this, value as IXSComponent);
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
