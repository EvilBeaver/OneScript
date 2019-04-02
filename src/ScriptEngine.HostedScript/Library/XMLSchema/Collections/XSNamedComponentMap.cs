/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;


namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("КоллекцияИменованныхКомпонентXS", "XSNamedComponentMap")]
    public class XSNamedComponentMap : AutoContext<XSNamedComponentMap>, ICollectionContext, IEnumerable<IXSNamedComponent>
    {
        private readonly List<IXSNamedComponent> _items;
      
        internal XSNamedComponentMap() => _items = new List<IXSNamedComponent>();

        internal void Add(IXSNamedComponent value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
                return;

            _items.Add(value);
        }

        internal void Delete(IXSNamedComponent value) => _items.Remove(value);

        internal void Clear() => _items.Clear();

        #region OneScript

        #region Methods

        [ContextMethod("Количество", "Count")]
        public int Count() => _items.Count;

        [ContextMethod("Получить", "Get")]
        public IXSNamedComponent Get(IValue value)
        {
            DataType DataType = value.DataType;
            switch (DataType)
            {
                case DataType.String:
                    return _items.FirstOrDefault(x => x.Name.Equals(value.AsString()));

                case DataType.Number:
                    return _items[(int)value.AsNumber()];

                default:
                    throw RuntimeException.InvalidArgumentType();
            }
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
