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
    [ContextClass("ЗаписьКаталога", "DirectoryEntry")]
    class DirectoryEntryImpl : AutoContext<DirectoryEntryImpl>
    {
        public readonly DirectoryEntry _directoryEntry;

        [ContextProperty("Имя", "Name")]
        public string Name => _directoryEntry.Name;

        [ContextProperty("Свойства", "Properties")]
        public PropertyCollectionImpl Properties => new PropertyCollectionImpl(_directoryEntry.Properties);

        [ContextProperty("ИмяПользователя", "Username")]
        public string Username
        {
            get { return _directoryEntry.Username; }
            set { _directoryEntry.Username = value; }
        }

        [ContextProperty("Пароль", "Password")]
        public string Password
        {
            get { return "********"; }
            set { _directoryEntry.Password = value; }
        }

        #region Constructors

        #region Impl

        public DirectoryEntryImpl(DirectoryEntry directoryEntry)
        {
            _directoryEntry = directoryEntry;
        }

        public DirectoryEntryImpl()
        {
            _directoryEntry = new DirectoryEntry();
        }

        public DirectoryEntryImpl(string path)
        {
            _directoryEntry = new DirectoryEntry(path);
        }

        public DirectoryEntryImpl(string path, string username, string password)
        {
            _directoryEntry = new DirectoryEntry(path, username, password);
        }

        #endregion

        /// <summary>
        /// Конструктор создания записи каталога без привязки.
        /// </summary>
        [ScriptConstructor(Name = "Без привязки")]
        public static DirectoryEntryImpl Constructor()
        {
            var direntry = new DirectoryEntryImpl();
            return direntry;
        }

        /// <summary>
        /// Конструктор создания записи каталога по заданному пути.
        /// </summary>
        /// <param name="name">Путь к объекту в дереве каталога.</param>
        [ScriptConstructor(Name = "По пути объекта")]
        public static DirectoryEntryImpl Constructor(IValue name)
        {
            var direntry = new DirectoryEntryImpl(name.AsString());
            return direntry;
        }

        /// <summary>
        /// Конструктор создания записи каталога по заданному пути с указанием имени пользователя и пароля.
        /// </summary>
        /// <param name="name">Путь к объекту в дереве каталога.</param>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="password">Пароль.</param>
        [ScriptConstructor(Name = "По пути объекта, имени пользователя и паролю")]
        public static DirectoryEntryImpl Constructor(IValue name, IValue username, IValue password)
        {
            var direntry = new DirectoryEntryImpl(name.AsString(), username.AsString(), password.AsString());
            return direntry;
        }

        #endregion

        /// <summary>
        /// Проверяет существование записи каталога по заданному пути.
        /// </summary>
        /// <param name="path">Путь к объекту в дереве каталога.</param>
        [ContextMethod("Существует", "Exists")]
        public static bool Exists(IValue path)
        {
            return DirectoryEntry.Exists(path.AsString());
        }
    }
}
