/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    public class LibraryResolver : IDirectiveResolver
    {
        private const string USE_DIRECTIVE_RU = "использовать";
        private const string USE_DIRECTIVE_EN = "use";
        private const string PREDEFINED_LOADER_FILE = "package-loader.os";

        private readonly RuntimeEnvironment _env;
        private readonly ScriptingEngine _engine;
        private readonly List<Library> _libs;
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
        private readonly Stack<ICodeSource> _compiledSourcesStack = new Stack<ICodeSource>();

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
            TraceLoadLibrary(
                Locale.NStr($"ru = 'Путь поиска package-loader - {loaderscript}';"+
                            $"en = 'Package-loader path search - {loaderscript}'")
            );

            if (File.Exists(loaderscript))
            {
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Загружен package-loader по адресу {loaderscript}';"+
                                $"en = 'Load package-loader from {loaderscript}'")
                );
                _defaultLoader = LibraryLoader.Create(_engine, _env, loaderscript);
            }
            else
            {
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Загружен package-loader по умолчанию';"+
                                $"en = 'Default package-loader is used'")
                );
                _defaultLoader = LibraryLoader.Create(_engine, _env);
            }
        }

        public bool Resolve(string directive, string value, bool codeEntered)
        {
            if (codeEntered) {
                return false;
            }

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
            bool quoted = PrepareQuoted(ref value);
            bool loaded;
            if (quoted)
                loaded = LoadByRelativePath(value);
            else
                loaded = LoadByName(value);

            if(!loaded)
                throw new CompilerException(String.Format("Библиотека не найдена: '{0}'", value));

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
                    realPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentPath), libraryPath));
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

        private bool PrepareQuoted(ref string value)
        {
            const string COMMENT = "//";
            const char QUOTE = '"';

            bool quoted = false;
            if (value.IndexOf(QUOTE)==0)
            {
                var secondQuote = value.Substring(1).IndexOf(QUOTE);
                if (secondQuote > 0)
                {
                    if (secondQuote+2 < value.Length)
                    {
                        var tail = value.Substring(secondQuote+2, value.Length-secondQuote-2).TrimStart();
                        if (!String.IsNullOrWhiteSpace(tail) && tail.IndexOf(COMMENT) != 0)
                            throw new CompilerException($"Недопустимые символы после имени библиотеки: '{tail}'");
                    }
                    value = value.Substring(1, secondQuote);
                    quoted = true;
                }
                else
                {
                    throw new CompilerException($"Ошибка в имени библиотеки: '{value}'");
                }
            }
            else
            {
                var comment = value.IndexOf(COMMENT);
                if( comment>=0 )
                {
                    value = value.Substring(0,comment).TrimEnd();
                }
            }

            if (String.IsNullOrWhiteSpace(value))
                throw new CompilerException("Отсутствует имя библиотеки");

            return quoted;
        }

        private bool LoadByPath(string libraryPath)
        {
            if (Directory.Exists(libraryPath))
            {
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Загружаю библиотеку по пути {libraryPath}';"+
                                $"en = 'Load library from path {libraryPath}'")
                );
                return LoadLibraryInternal(libraryPath);
            }

            return false;
        }

        private bool LoadByName(string value)
        {
            foreach (var path in SearchDirectories)
            {
                if(!Directory.Exists(path))
                    continue;

                var libraryPath = Path.Combine(path, value);
                if (LoadByPath(libraryPath))
                    return true;
            }

            var rootPath = Path.Combine(LibraryRoot, value);
            if (LoadByPath(rootPath))
                return true;

            return false;
        }

        private bool LoadLibraryInternal(string libraryPath)
        {
            var id = GetLibraryId(libraryPath);
            var existedLib = _libs.FirstOrDefault(x => x.id == id);
            if(existedLib != null)
            {
                if (existedLib.state == ProcessingState.Discovered)
                {
                    string libStack = listToStringStack(_libs, id);
                    throw new RuntimeException($"Ошибка загрузки библиотеки {id}. Обнаружены циклические зависимости.\n" +
                                               $"{libStack}");
                }
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Использую уже загруженную библиотеку {existedLib.id}';"+
                                $"en = 'Use allready loaded library {existedLib.id}'")
                );
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
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Начинаю процессинг {newLib.id}';"+
                                $"en = 'Start processing {newLib.id}'")
                );

                hasFiles = ProcessLibrary(newLib);
                newLib.state = ProcessingState.Processed;
            }
            catch (Exception)
            {
                _libs.RemoveAt(newLibIndex);
                throw;
            }

            TraceLoadLibrary(
                Locale.NStr($"ru = 'Библиотека {newLib.id} будет загружена - {hasFiles}';"+
                            $"en = 'Library {newLib.id} will be loaded - {hasFiles}'")    
            );

            return hasFiles;
        }

        private string listToStringStack(List<Library> libs, string stopToken)
        {
            var builder = new StringBuilder();
            string offset = "";
            foreach (var library in libs)
            {
                builder.Append(offset);
                builder.Append("-> ");
                builder.AppendLine(library.id);
                offset += "  ";
                if (library.id == stopToken)
                {
                    break;
                }
            }

            return builder.ToString();
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

        public static void TraceLoadLibrary(string message)
        {
            //OS_LRE_TRACE - по аналогии с Package loader OSLIB_LOADER_TRACE
            var isTrace = System.Environment.GetEnvironmentVariable("OS_LRE_TRACE");
            if (isTrace == "1") {
                Console.WriteLine("LRE: " + message);
            }
        }

    }
}
