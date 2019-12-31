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
        public string Filter => _directorySearcher.Filter;

        [ContextProperty("СвойстваДляПолучения", "PropertiesToLoad")]
        public ArrayImpl PropertiestoLoad { get; }
        

        #region Constructors

        #region inner constructors
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

        public DirectorySearcherImpl(string filter, string[] PropertiesToLoad)
        {
            _directorySearcher = new DirectorySearcher(filter, PropertiesToLoad);
        }

        public DirectorySearcherImpl(DirectoryEntryImpl directoryEntry, string filter, string[] propertiesToLoad)
        {
            _directorySearcher = new DirectorySearcher(directoryEntry._directoryEntry, filter, propertiesToLoad);
            SearchRoot = directoryEntry;
        }

        #endregion

        #region script constructors

        /// <summary>
        /// Конструктор создания поиска по каталогу без привязок.
        /// </summary>
        [ScriptConstructor(Name = "Без привязок")]
        public static DirectorySearcherImpl Constructor()
        {
            var direntry = new DirectorySearcherImpl();
            return direntry;
        }

        /// <summary>
        /// Конструктор создания поиска по каталогу с указанием корня поиска.
        /// <param name="directoryEntry">Путь к объекту в дереве каталога.</param>
        /// </summary>
        [ScriptConstructor(Name = "По записи каталога")]
        public static DirectorySearcherImpl Constructor(IValue directoryEntry)
        {
            if (!(directoryEntry.GetRawValue() is DirectoryEntryImpl val))
            {
                throw RuntimeException.InvalidArgumentType();
            }
            var dirseacrh = new DirectorySearcherImpl(val);
            return dirseacrh;
        }

        /// <summary>
        /// Конструктор создания поиска по каталогу с указанием фильтра.
        /// <param name="filter">Путь к объекту в дереве каталога.</param>
        /// </summary>
        [ScriptConstructor(Name = "По фильтру")]
        public static DirectorySearcherImpl Constructor(IValue filter)
        {
            var dirseacrh = new DirectorySearcherImpl(filter.AsString());
            return dirseacrh;
        }

        /// <summary>
        /// Конструктор создания поиска по каталогу с указанием корня поиска и фильтра.
        /// </summary>
        [ScriptConstructor(Name = "По записи каталога и фильтру")]
        public static DirectorySearcherImpl Constructor(IValue directoryEntry, IValue filter)
        {
            if (!(directoryEntry.GetRawValue() is DirectoryEntryImpl val))
            {
                throw RuntimeException.InvalidArgumentType();
            }
            var dirseacrh = new DirectorySearcherImpl(val, filter.AsString());
            return dirseacrh;
        }

        /// <summary>
        /// Конструктор создания поиска по каталогу с указанием фильтра и набора полей для получения.
        /// </summary>
        [ScriptConstructor(Name = "по фильтру и набору полей")]
        public static DirectorySearcherImpl Constructor(IValue filter, IValue propertiesToLoad)
        {
            string[] props = ;
            var dirseacrh = new DirectorySearcherImpl(filter.AsString(), props);
            return dirseacrh;
        }

        /// <summary>
        /// Конструктор создания поиска по каталогу с указанием .
        /// </summary>
        [ScriptConstructor(Name = "Без привязки")]
        public static DirectorySearcherImpl Constructor()
        {
            var dirseacrh = new DirectorySearcherImpl();
            return dirseacrh;
        }

        #endregion

        #endregion
    }
}
