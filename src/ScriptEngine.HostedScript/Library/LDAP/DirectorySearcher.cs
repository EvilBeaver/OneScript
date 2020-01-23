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
    [ContextClass("ПоискВКаталоге", "DirectorySearcher")]
    class DirectorySearcherImpl : AutoContext<DirectorySearcherImpl>
    {
        private readonly DirectorySearcher _directorySearcher;

        [ContextProperty("КореньПоиска", "SearchRoot")]
        public DirectoryEntryImpl SearchRoot { get; set; }

        [ContextProperty("Фильтр", "Filter")]
        public string Filter
        {
            get { return _directorySearcher.Filter; }
            set {  _directorySearcher.Filter = value; }
        }

        [ContextProperty("СвойстваДляПолучения", "PropertiesToLoad")]
        public ArrayImpl PropertiestoLoad { get; }
        

        #region Constructors

        #region Impl
        public DirectorySearcherImpl()
        {
            _directorySearcher = new DirectorySearcher();
        }

        public DirectorySearcherImpl(DirectoryEntryImpl directoryEntry)
        {
            _directorySearcher = new DirectorySearcher(directoryEntry._directoryEntry);
            SearchRoot = directoryEntry;
        }

        public DirectorySearcherImpl(string filter)
        {
            _directorySearcher = new DirectorySearcher(filter);

        }

        public DirectorySearcherImpl(DirectoryEntryImpl directoryEntry, string filter)
        {
            _directorySearcher = new DirectorySearcher(directoryEntry._directoryEntry, filter);
            SearchRoot = directoryEntry;
        }

        public DirectorySearcherImpl(string filter, ArrayImpl propsToLoad)
        {
            _directorySearcher = new DirectorySearcher(filter, propsToLoad.Select(p => p.AsString()).ToArray());
            PropertiestoLoad = propsToLoad;
        }

        public DirectorySearcherImpl(DirectoryEntryImpl directoryEntry, string filter, ArrayImpl propsToLoad)
        {
            _directorySearcher = new DirectorySearcher(directoryEntry._directoryEntry, filter, propsToLoad.Select(p => p.AsString()).ToArray());
            SearchRoot = directoryEntry;
            PropertiestoLoad = propsToLoad;
        }

        #endregion

        #region script constructors

        /// <summary>
        /// Конструктор создания поиска по каталогу с указанием корня поиска.
        /// <param name="searchRoot">Путь к корневому объекту поиска в дереве каталога.</param>
        /// <param name="filter">Строка, содержащая фильтр.</param>
        /// <param name="propertiesToLoad">Массив имён свойств, которые нужно получать при поиске.</param>
        /// </summary>
        [ScriptConstructor(Name = "По записи каталога")]
        public static DirectorySearcherImpl Constructor(IValue searchRoot = null, string filter = null, IValue propertiesToLoad = null)
        {
            DirectoryEntryImpl dirEntry = null;
            ArrayImpl propsToLoad = new ArrayImpl();
            if ((searchRoot ?? ValueFactory.Create()).GetRawValue() is DirectoryEntryImpl val_de)
            {
                dirEntry = val_de;
            } else if (searchRoot != null)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            if ((propertiesToLoad ?? ValueFactory.Create()).GetRawValue() is ArrayImpl val_ptl)
            {
                propsToLoad = val_ptl;
            }
            else if (propertiesToLoad != null)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var dirseacrh = new DirectorySearcherImpl(dirEntry, filter ?? "(objectClass=*)", propsToLoad);
            return dirseacrh;
        }


        #endregion

        #endregion

        [ContextMethod("НайтиОдин", "FindOne")]
        public SearchResultImpl FindOne()
        {
            //return new DirectoryEntryImpl(_directorySearcher.FindOne().GetDirectoryEntry());
             return new SearchResultImpl(_directorySearcher.FindOne());
        }

    }
}
