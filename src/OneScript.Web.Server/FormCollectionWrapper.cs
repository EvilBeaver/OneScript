/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using OneScript.StandardLibrary.Collections;
using System.Collections.Generic;
using OneScript.StandardLibrary.Binary;
using Microsoft.Extensions.Primitives;

namespace OneScript.Web.Server
{
    [ContextClass("Форма", "Form")]
    public class FormCollectionWrapper : AutoCollectionContext<FormCollectionWrapper, KeyAndValueImpl>
    {
        private readonly IFormCollection _items;

        internal FormCollectionWrapper(IFormCollection headers)
        {
            _items = headers;
        }

        public override bool IsIndexed => true;

        public override StringValuesWrapper GetIndexedValue(IValue index)
        {
            if (_items.TryGetValue(index.AsString(), out var result))
                return result;
            else
                return StringValues.Empty;
        }

        internal bool ContainsKey(IValue key)
        {
            return _items.ContainsKey(key.AsString());
        }

        public IEnumerable<IValue> Keys()
        {
            foreach (var key in _items.Keys)
                yield return ValueFactory.Create(key);
        }

        #region ICollectionContext Members

        [ContextMethod("Получить", "Get")]
        public StringValuesWrapper Get(IValue key)
        {
            return GetIndexedValue(key);
        }

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _items.Count;
        }

        #endregion

        #region IEnumerable<IValue> Members

        public override IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return new KeyAndValueImpl(ValueFactory.Create(item.Key), (StringValuesWrapper)item.Value);
            }
        }

        #endregion

        [ContextProperty("Файлы", "Files", CanWrite = false)]
        public FormFileCollectionWrapper Files => new(_items.Files);
    }
}
