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
