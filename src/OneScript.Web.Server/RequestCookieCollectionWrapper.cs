/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;

namespace OneScript.Web.Server
{
    [ContextClass("КукиЗапроса", "RequestCookies")]
    public class RequestCookieCollectionWrapper : AutoCollectionContext<RequestCookieCollectionWrapper, KeyAndValueImpl>
    {
        private readonly IRequestCookieCollection _items;

        public string this[string key] => _items[key];

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.SystemType != BasicTypes.String)
                throw RuntimeException.InvalidArgumentType();
            
            return BslStringValue.Create(this[index.ToString()]);
        }

        public override bool IsIndexed => true;

        public RequestCookieCollectionWrapper(IRequestCookieCollection headers)
        {
            _items = headers;
        }

        public override int Count() => _items.Count;

        public override IEnumerator<KeyAndValueImpl> GetEnumerator()
            => _items.Select(c => new KeyAndValueImpl(BslStringValue.Create(c.Key), BslStringValue.Create(c.Value))).GetEnumerator();
    }
}
