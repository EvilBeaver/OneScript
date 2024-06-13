using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Web.Server;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;
using OneScript.StandardLibrary.Collections;

namespace OneScript.Web.Server
{
    [ContextClass("СловарьЗаголовков", "HeaderDictionary")]
    public class HeaderDictionaryWrapper : AutoCollectionContext<HeaderDictionaryWrapper, KeyAndValueImpl>
    {
        private readonly IHeaderDictionary _items;

        public IValue this[IValue key]
        {
            get => BslStringValue.Create(_items[key.AsString()]);
            set => _items[key.AsString()] = value.AsString();
        }

        public HeaderDictionaryWrapper(IHeaderDictionary headers)
        {
            _items = headers;
        }

        public override int Count() => _items.Count;

        public override IEnumerator<KeyAndValueImpl> GetEnumerator() 
            => _items.Select(c => new KeyAndValueImpl(BslStringValue.Create(c.Key), BslStringValue.Create(c.Value))).GetEnumerator();
    }
}
