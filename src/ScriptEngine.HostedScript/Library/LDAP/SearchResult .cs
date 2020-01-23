using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.DirectoryServices;

namespace ScriptEngine.HostedScript.Library.LDAP
{
    [ContextClass("РезультатПоиска", "SearchResult")]
    class SearchResultImpl : AutoContext<SearchResultImpl>
    {
        private readonly SearchResult _searchResult;

        [ContextProperty("Путь", "Path")]
        public string Path => _searchResult.Path;

        [ContextProperty("Свойства", "Properties")]
        public MapImpl Properties
        { get {
                MapImpl result = new MapImpl();
                foreach (string key in _searchResult.Properties.PropertyNames) {
                    result.Insert(ValueFactory.Create(key), ResultValueCollectionAsIValue(_searchResult.Properties[key]));
                }
                return result;
            }
        }

        private IValue ResultValueCollectionAsIValue(ResultPropertyValueCollection values)
        {
            IValue result;
            if (values.Count == 1)
            {
                result = ValueFactory.Create(values[0].ToString());
            }
            else
            {
                ArrayImpl collection = new ArrayImpl();
                foreach (object value in values)
                {
                    collection.Add(ValueFactory.Create(value.ToString()));
                }
                result = collection;
            }
            return result;
        }

        [ContextMethod("ПолучитьЗаписьКаталога", "GetDirectoryEntry")]
        public DirectoryEntryImpl GetDirectoryEntry()
        {
            return new DirectoryEntryImpl(_searchResult.GetDirectoryEntry());
        }

        #region Constructors
        #region Impl
        public SearchResultImpl(SearchResult searchResult)
        {
            _searchResult = searchResult;
        }
        #endregion
        #endregion
    }
}
