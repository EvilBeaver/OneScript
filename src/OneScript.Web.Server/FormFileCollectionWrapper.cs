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
using System.Collections.Generic;
using OneScript.Values;
using OneScript.StandardLibrary.Collections;

namespace OneScript.Web.Server
{
    [ContextClass("ФайлыФормы", "FormFiles")]
    public class FormFileCollectionWrapper : AutoCollectionContext<FormFileCollectionWrapper, FormFileWrapper>
    {
        private readonly IFormFileCollection _items;

        internal FormFileCollectionWrapper(IFormFileCollection items)
        {
            _items = items;
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            var result = _items.GetFile(index.AsString());

            if (result == null)
                return BslUndefinedValue.Instance;
            else
                return new FormFileWrapper(result);
        }

        #region ICollectionContext Members

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
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

        public override IEnumerator<FormFileWrapper> GetEnumerator()
        {
            foreach (var item in _items)
                yield return new FormFileWrapper(item);
        }

        #endregion
    }
}
