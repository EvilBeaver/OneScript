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
    [ContextClass("ПоискВКаталогеLDAP", "LDAPDirectorySearcher")]
    class LDAPDirectorySearcherImpl : AutoContext<LDAPDirectorySearcherImpl>
    {
        private readonly DirectorySearcher _directorySearcher;

        [ContextProperty("КореньПоиска", "SearchRoot")]
        public LDAPDirectoryEntryImpl SearchRoot { get; set; }

        [ContextProperty("Фильтр", "Filter")]
        public string Filter
        {
            get { return _directorySearcher.Filter; }
            set {  _directorySearcher.Filter = value; }
        }

        [ContextProperty("СвойстваДляПолучения", "PropertiesToLoad")]
        public ArrayImpl PropertiesToLoad { get; }


        [ContextProperty("ОбластьПоиска", "SearchScope")]
        public LDAPSearchScopeImpl SearchScope {
            get { return SearchScopeConverter.ToSearchScopeImpl(_directorySearcher.SearchScope); } 
            set { _directorySearcher.SearchScope = SearchScopeConverter.ToSearchScope(value); }
        }

        #region Constructors

        #region Impl

        public LDAPDirectorySearcherImpl(LDAPDirectoryEntryImpl directoryEntry, string filter, ArrayImpl propsToLoad, LDAPSearchScopeImpl searchScope)
        {
            _directorySearcher = new DirectorySearcher(directoryEntry._directoryEntry, filter, propsToLoad.Select(p => p.AsString()).ToArray(), SearchScopeConverter.ToSearchScope(searchScope));
            SearchRoot = directoryEntry;
            PropertiesToLoad = propsToLoad;
            SearchScope = searchScope;
        }

        #endregion

        #region script constructors

        /// <summary>
        /// Конструктор создания поиска по каталогу.
        /// <param name="searchRoot">Путь к корневому объекту поиска в дереве каталога.</param>
        /// <param name="filter">Строка, содержащая фильтр.</param>
        /// <param name="propertiesToLoad">Массив имён свойств, которые нужно получать при поиске.</param>
        /// <param name="searchScope">Значение перечисления ОбластьПоиска, по-умолчанию Дерево.</param>
        /// </summary>
        [ScriptConstructor]
        public static LDAPDirectorySearcherImpl Constructor(IValue searchRoot = null, string filter = "(objectClass=*)", IValue propertiesToLoad = null, LDAPSearchScopeImpl searchScope = LDAPSearchScopeImpl.Subtree)
        {
            LDAPDirectoryEntryImpl dirEntry = null;
            ArrayImpl propsToLoad = new ArrayImpl();
            if (searchRoot?.GetRawValue() is LDAPDirectoryEntryImpl val_de)
            {
                dirEntry = val_de;
            } else if (searchRoot != null)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            if (propertiesToLoad?.GetRawValue() is ArrayImpl val_ptl)
            {
                propsToLoad = val_ptl;
            }
            else if (propertiesToLoad != null)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var dirseacrh = new LDAPDirectorySearcherImpl(dirEntry, filter, propsToLoad, searchScope);
            return dirseacrh;
        }


        #endregion

        #endregion

        [ContextMethod("НайтиОдин", "FindOne")]
        public IValue FindOne()
        {
            SearchResult searchResult = _directorySearcher.FindOne();
            if (searchResult == null)
            {
                return null;
            }
            else
            {
                return new LDAPSearchResultImpl(searchResult);
            }
        }

        [ContextMethod("НайтиВсе", "FindAll")]
        public ArrayImpl FindAll()
        {
            SearchResultCollection searchResultCollection = _directorySearcher.FindAll();
            ArrayImpl result = new ArrayImpl();
            foreach (SearchResult searchResult in searchResultCollection)
            {
                result.Add(new LDAPSearchResultImpl(searchResult));
            }
            return result;
        }

    }
}
