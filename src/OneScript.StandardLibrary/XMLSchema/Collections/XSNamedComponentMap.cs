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
using OneScript.Core;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Collections
{
    [ContextClass("КоллекцияИменованныхКомпонентXS", "XSNamedComponentMap")]
    public class XSNamedComponentMap : AutoCollectionContext<XSNamedComponentMap, IXSNamedComponent>
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
        public override int Count() => _items.Count;

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

        public override IEnumerator<IXSNamedComponent> GetEnumerator() => _items.GetEnumerator();

    }
}
