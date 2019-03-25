using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;


namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("КоллекцияИменованныхКомпонентXS", "XSNamedComponentMap")]
    public class XSNamedComponentMap : AutoContext<XSNamedComponentMap>, ICollectionContext, IEnumerable<IXSNamedComponent>
    {
        private readonly List<IXSNamedComponent> _items;

        internal XSNamedComponentMap() => _items = new List<IXSNamedComponent>();

        internal void Add(IXSNamedComponent value) => _items.Add(value);
        internal void Delete(IXSNamedComponent value) => _items.Remove(value);

        #region OneScript

        #region Methods

        [ContextMethod("Количество", "Count")]
        public int Count() => _items.Count;

        [ContextMethod("Получить", "Get")]
        public IXSNamedComponent Get(int index) => _items[index];

        public IXSNamedComponent Get(string name)
        {
            throw new NotImplementedException();
        }

        public IXSNamedComponent Get(string name, string namespaceURI)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region ICollectionContext

        public CollectionEnumerator GetManagedIterator() => new CollectionEnumerator(GetEnumerator());

        #endregion

        #region IEnumerable

        public IEnumerator<IXSNamedComponent> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

    }
}
