﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФиксированныйСписокКомпонентXS", "XSComponentFixedList")]
    public class XSComponentFixedList : AutoContext<XSComponentFixedList>, ICollectionContext, IEnumerable<IXSComponent>
    {

        private readonly List<IXSComponent> _items;

        public XSComponentFixedList() => _items = new List<IXSComponent>();

        public void Add(IXSComponent value) => _items.Add(value);
        public void Remove(IXSComponent value) => _items.Remove(value);
        public void Clear() => _items.Clear();
        public void RemoveAll(Predicate<IXSComponent> predicate) => _items.RemoveAll(predicate);

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
