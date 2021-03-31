/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Core;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [ContextClass("КонтекстПространствИменXML", "XMLNamespaceContext")]
    public class XmlNamespaceContext : AutoContext<XmlNamespaceContext>
    {
        readonly IDictionary<string, string> _nsmap;

        public XmlNamespaceContext(int depth, IDictionary<string, string> map)
        {
            Depth = depth;
            _nsmap = map;
        }

        [ContextProperty("Глубина", "Depth")]
        public int Depth { get; }

        [ContextProperty("ПространствоИменПоУмолчанию", "DefaultNamespace")]
        public string DefaultNamespace
        {
            get
            {
                if (_nsmap.ContainsKey(""))
                    return _nsmap[""];
                return "";
            }
        }

        [ContextMethod("URIПространствИмен", "NamespaceURIs")]
        public ArrayImpl NamespaceUris()
        {
            var result = ArrayImpl.Constructor() as ArrayImpl;
            foreach (var ns in _nsmap.Values.Distinct())
            {
                result.Add(ValueFactory.Create(ns));
            }
            return result;
        }

        [ContextMethod("НайтиURIПространстваИмен", "LookupNamespaceURI")]
        public IValue LookupNamespaceUri(string prefix)
        {
            if (_nsmap.ContainsKey(prefix))
                return ValueFactory.Create(_nsmap[prefix]);
            return ValueFactory.Create();
        }

        [ContextMethod("НайтиПрефикс", "LookupPrefix")]
        public IValue LookupPrefix(string namespaceUri)
        {
            foreach (var kv in _nsmap)
            {
                if (kv.Value.Equals(namespaceUri, StringComparison.Ordinal))
                    return ValueFactory.Create(kv.Key);
            }
            return ValueFactory.Create();
        }

        [ContextMethod("Префиксы", "Prefixes")]
        public ArrayImpl Prefixes(string namespaceUri)
        {
            var result = ArrayImpl.Constructor() as ArrayImpl;
            foreach (var prefix in _nsmap
                     .Where((arg) => arg.Value.Equals(namespaceUri, StringComparison.Ordinal))
                     .Select((arg) => arg.Key))
            {
                result.Add(ValueFactory.Create(prefix));
            }

            return result;
        }

        [ContextMethod("СоответствияПространствИмен", "NamespaceMappings")]
        public MapImpl NamespaceMappings()
        {
            var result = MapImpl.Constructor() as MapImpl;
            foreach (var data in _nsmap)
            {
                result.Insert(ValueFactory.Create(data.Key), ValueFactory.Create(data.Value));
            }

            return result;
        }
    }
}
