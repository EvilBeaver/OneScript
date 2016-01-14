/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
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
        private const string PREDEFINED_LOADER_FILE = "package-loader.os";

        private RuntimeEnvironment _env;
        private ScriptingEngine _engine;
        private List<Library> _libs;
        private LibraryLoader _defaultLoader;
        private string _libraryRoot;

        #region Private classes

        private class Library
        {
            public string id;
            public ProcessingState state;
            public LibraryLoader customLoader;
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

            this.SearchDirectories = new List<string>();
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

        public List<string> SearchDirectories { get; private set; }

        //TODO: Тут совсем ужасно спроектировано взаимодействие слоев и передача контекста
        // нужно снова заняться версией 2.0 ((
        private Stack<ICodeSource> _compiledSourcesStack = new Stack<ICodeSource>();

        public ICodeSource Source 
        { 
            get
            {
                if (_compiledSourcesStack.Count == 0)
                    return null;

                return _compiledSourcesStack.Peek();
            }
            set
            {
                if(value == null)
                {
                    if (_compiledSourcesStack.Count > 0)
                        _compiledSourcesStack.Pop();
                }
                else
                {
                    _compiledSourcesStack.Push(value);
                }
            }
        }

        private LibraryLoader DefaultLoader
        {
            get 
            {
                if (_defaultLoader == null)
                    CreateDefaultLoader();

                return _defaultLoader;
                
            }
            set { _defaultLoader = value; }
        }

        private void CreateDefaultLoader()
        {

            var loaderscript = Path.Combine(LibraryRoot, PREDEFINED_LOADER_FILE);
            if (File.Exists(loaderscript))
            {
                _defaultLoader = LibraryLoader.Create(_engine, _env, loaderscript);
            }
            else
            {
                _defaultLoader = LibraryLoader.Create(_engine, _env);
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

            bool loaded;
            if (IsQuoted(value))
                loaded = LoadByRelativePath(value.Substring(1, value.Length - 2));
            else
                loaded = LoadByName(value);

            if(!loaded)
                throw new CompilerException(String.Format("Библиотека не найдена {0}", value));

        }

        private bool LoadByRelativePath(string libraryPath)
        {
            string realPath;

            if (!Path.IsPathRooted(libraryPath) && Source != null)
            {
                var currentPath = Source.SourceDescription;
                // Загружаем относительно текущего скрипта, однако,
                // если CurrentScript не файловый (TestApp или другой хост), то загружаем относительно рабочего каталога.
                // немного костыльно, ага ((
                //
                if (!PathHasInvalidChars(currentPath))
                    realPath = Path.Combine(Path.GetDirectoryName(currentPath), libraryPath);
                else
                    realPath = libraryPath;
            }
            else
            {
                realPath = libraryPath;
            }

            return LoadByPath(realPath);
        }

        private static bool PathHasInvalidChars(string path)
        {

            return (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0);
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
            var rootPath = Path.Combine(LibraryRoot, value);
            if (LoadByPath(rootPath))
                return true;
            
            foreach (var path in SearchDirectories)
            {
                if(!Directory.Exists(path))
                    continue;

                var libraryPath = Path.Combine(path, value);
                if (LoadByPath(libraryPath))
                    return true;
            }

            return false;
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
            bool hasFiles;
            int newLibIndex = _libs.Count;
            
            var customLoaderFile = Path.Combine(libraryPath, PREDEFINED_LOADER_FILE);
            if (File.Exists(customLoaderFile))
                newLib.customLoader = LibraryLoader.Create(_engine, _env, customLoaderFile);

            try
            {
                _libs.Add(newLib);
                hasFiles = ProcessLibrary(newLib);
                newLib.state = ProcessingState.Processed;
            }
            catch (Exception)
            {
                _libs.RemoveAt(newLibIndex);
                throw;
            }

            return hasFiles;
        }

        private string GetLibraryId(string libraryPath)
        {
            return Path.GetFullPath(libraryPath);
        }

        private bool ProcessLibrary(Library lib)
        {
            LibraryLoader loader;
            if (lib.customLoader != null)
                loader = lib.customLoader;
            else
                loader = this.DefaultLoader;

            return loader.ProcessLibrary(lib.id);
        }

    }
}
