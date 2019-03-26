using System;
using System.Collections;
using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФиксированныйСписокКомпонентXS", "XSComponentFixedList")]
    public class XSComponentFixedList : AutoContext<XSComponentFixedList>, ICollectionContext, IEnumerable<IXSComponent>
    {

        private readonly List<IXSComponent> _items;

        public XSComponentFixedList() => _items = new List<IXSComponent>();

        public void Add(IXSComponent value) => _items.Add(value);
        public void Clear() => _items.Clear();

        #region OneScript

        #region Methods

        [ContextMethod("Количество", "Count")]
        public int Count() => _items.Count;

        [ContextMethod("Получить", "Get")]
        public IXSComponent Get(int index) => _items[index];

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent value) => _items.Contains(value);

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
