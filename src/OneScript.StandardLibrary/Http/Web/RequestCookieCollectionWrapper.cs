using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;

namespace OneScript.StandardLibrary.Http.Web
{
    [ContextClass("КукиЗапроса", "RequestCookies")]
    public class RequestCookieCollectionWrapper : AutoCollectionContext<RequestCookieCollectionWrapper, KeyAndValueImpl>
    {
        private readonly IRequestCookieCollection _items;

        public IValue this[IValue key]
        {
            get => BslStringValue.Create(_items[key.AsString()]);
        }

        public RequestCookieCollectionWrapper(IRequestCookieCollection headers)
        {
            _items = headers;
        }

        public override int Count() => _items.Count;

        public override IEnumerator<KeyAndValueImpl> GetEnumerator()
            => _items.Select(c => new KeyAndValueImpl(BslStringValue.Create(c.Key), BslStringValue.Create(c.Value))).GetEnumerator();
    }
}
