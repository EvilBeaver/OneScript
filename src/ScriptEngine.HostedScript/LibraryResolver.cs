/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
        private const string PREDEFINED_LOADER_FILE = "package-loader.os";

        private RuntimeEnvironment _env;
        private ScriptingEngine _engine;
        private List<Library> _libs;
        private LibraryLoader _defaultLoader;
        private string _systemLibraryDir;

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

        public string SystemLibraryDir
        {
            get
            {
                if (_systemLibraryDir == null)
                    _systemLibraryDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                return _systemLibraryDir;
            }

            set
            {
                _systemLibraryDir = value;
            }
        }

        public List<string> SearchDirectories { get; private set; }

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

            var loaderscript = Path.Combine(SystemLibraryDir, PREDEFINED_LOADER_FILE);
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
            if (SearchDirectories.Count == 0)
            {
                var libraryPath = Path.Combine(SystemLibraryDir, value);
                return LoadByPath(libraryPath);
            }
            else
            {
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

            var customLoaderFile = Path.Combine(libraryPath, PREDEFINED_LOADER_FILE);
            if (File.Exists(customLoaderFile))
                newLib.customLoader = LibraryLoader.Create(_engine, _env, customLoaderFile);

            _libs.Add(newLib);

            bool hasFiles = ProcessLibrary(newLib);

            newLib.state = ProcessingState.Processed;

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
