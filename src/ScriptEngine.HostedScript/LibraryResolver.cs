using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    class LibraryResolver : IDirectiveResolver
    {
        private const string USE_DIRECTIVE_RU = "использовать";
        private const string USE_DIRECTIVE_EN = "use";

        private RuntimeEnvironment _env;
        private ScriptingEngine _engine;
        private List<Library> _libs;
        private LibraryLoader _loader;
        private string _libraryRoot;

        #region Private classes

        private class Library
        {
            public string id;
            public ProcessingState state;
        }

        private enum ProcessingState
        {
            Discovered,
            Processed
        }

        #endregion

        public LibraryResolver(ScriptingEngine engine, RuntimeEnvironment env)
        {
            _env = env;
            _engine = engine;
            _libs = new List<Library>();
        }

        public string LibraryRoot 
        {
            get
            {
                if (_libraryRoot == null)
                    _libraryRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                return _libraryRoot;
            }

            set
            {
                _libraryRoot = value;
            }
        }

        private void CreateLoader()
        {
            if (_loader != null)
                return;

            var loaderscript = Path.Combine(LibraryRoot, "package-loader.os");
            if(File.Exists(loaderscript))
            {
                _loader = LibraryLoader.Create(_engine, _env, loaderscript);
            }
            else
            {
                _loader = LibraryLoader.Create(_engine, _env);
            }
        }

        public bool Resolve(string directive, string value)
        {
            if (DirectiveSupported(directive))
            {
                LoadLibrary(value);
                return true;
            }
            else
                return false;
        }

        private bool DirectiveSupported(string directive)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(directive, USE_DIRECTIVE_RU) == 0
                || StringComparer.InvariantCultureIgnoreCase.Compare(directive, USE_DIRECTIVE_EN) == 0;
        }

        private void LoadLibrary(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Ошибка в имени библиотеки", "value");

            CreateLoader();

            bool loaded;
            if (IsQuoted(value))
                loaded = LoadByPath(value.Substring(1, value.Length - 2));
            else
                loaded = LoadByName(value);

            if(!loaded)
                throw new CompilerException(String.Format("Библиотека не найдена {0}", value));

        }

        private bool IsQuoted(string value)
        {
            const char QUOTE = '"';
            if(value[0] == QUOTE && value.Length > 1)
            {
                return value[0] == QUOTE && value[value.Length - 1] == QUOTE;
            }
            else
                return false;
            
        }

        private bool LoadByPath(string libraryPath)
        {
            if (Directory.Exists(libraryPath))
            {
                return LoadLibraryInternal(libraryPath);
            }

            return false;
        }

        private bool LoadByName(string value)
        {
            var libraryPath = Path.Combine(LibraryRoot, value);
            return LoadByPath(libraryPath);
        }

        private bool LoadLibraryInternal(string libraryPath)
        {
            var id = GetLibraryId(libraryPath);
            var existedLib = _libs.FirstOrDefault(x => x.id == id);
            if(existedLib != null)
            {
                if (existedLib.state == ProcessingState.Discovered)
                    throw new RuntimeException(String.Format("Ошибка загрузки библиотеки {0}. Обнаружены циклические зависимости", id));
                else
                    return true;
            }

            var newLib = new Library() { id = id, state = ProcessingState.Discovered };
            _libs.Add(newLib);

            bool hasFiles = ProcessLibrary(libraryPath);

            newLib.state = ProcessingState.Processed;

            return hasFiles;
        }

        private string GetLibraryId(string libraryPath)
        {
            return Path.GetFullPath(libraryPath);
        }

        private bool ProcessLibrary(string libraryPath)
        {
            return _loader.ProcessLibrary(libraryPath);
        }

    }
}
