/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OneScript.Commons;
using OneScript.Sources;
using ScriptEngine.Compiler;

namespace ScriptEngine.HostedScript
{
    public class FileSystemDependencyResolver : IDependencyResolver
    {
        public const string PREDEFINED_LOADER_FILE = "package-loader.os";
        
        private readonly List<Library> _libs = new List<Library>();
        private readonly Lazy<LibraryLoader> _defaultLoader;

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

        public FileSystemDependencyResolver()
        {
            _defaultLoader = new Lazy<LibraryLoader>(CreateDefaultLoader);
        }
        
        public IList<string> SearchDirectories { get;} = new List<string>();

        public string LibraryRoot => SearchDirectories.FirstOrDefault() ?? string.Empty;
        
        private ScriptingEngine Engine { get; set; }
        
        public void Initialize(ScriptingEngine engine)
        {
            Engine = engine;
        }
        
        public ExternalLibraryDef Resolve(SourceCode module, string libraryName)
        {
            bool quoted = PrepareQuoted(ref libraryName);
            bool loaded;
            if (quoted)
                loaded = LoadByRelativePath(module, libraryName);
            else
                loaded = LoadByName(libraryName);

            if(!loaded)
                throw new CompilerException(String.Format("Библиотека не найдена: '{0}'", libraryName));

            return default;
        }

        private bool LoadByName(string libraryName)
        {
            foreach (var path in SearchDirectories)
            {
                if(!Directory.Exists(path))
                    continue;

                var libraryPath = Path.Combine(path, libraryName);
                if (LoadByPath(libraryPath))
                    return true;
            }

            var rootPath = Path.Combine(LibraryRoot, libraryName);
            if (LoadByPath(rootPath))
                return true;

            return false;
        }

        private bool LoadByRelativePath(SourceCode module, string libraryPath)
        {
            string realPath;

            if (!Path.IsPathRooted(libraryPath) && module.Location != null)
            {
                var currentPath = module.Location;
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

        private LibraryLoader CreateDefaultLoader()
        {
            var loaderscript = Path.Combine(LibraryRoot, PREDEFINED_LOADER_FILE);
            return File.Exists(loaderscript) ? 
                LibraryLoader.Create(Engine, loaderscript) 
                : LibraryLoader.Create(Engine);
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
                        if (!string.IsNullOrWhiteSpace(tail) && tail.IndexOf(COMMENT, StringComparison.Ordinal) != 0)
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
                var comment = value.IndexOf(COMMENT, StringComparison.Ordinal);
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
                return LoadLibraryInternal(libraryPath);
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
                {
                    string libStack = ListToStringStack(_libs, id);
                    throw new RuntimeException($"Ошибка загрузки библиотеки {id}. Обнаружены циклические зависимости.\n" +
                                               $"{libStack}");
                }
                
                return true;
            }

            var newLib = new Library() { id = id, state = ProcessingState.Discovered };
            bool hasFiles;
            int newLibIndex = _libs.Count;
            
            var customLoaderFile = Path.Combine(libraryPath, PREDEFINED_LOADER_FILE);
            if (File.Exists(customLoaderFile))
                newLib.customLoader = LibraryLoader.Create(Engine, customLoaderFile);

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

        private bool ProcessLibrary(Library lib)
        {
            LibraryLoader loader;
            if (lib.customLoader != null)
                loader = lib.customLoader;
            else
                loader = _defaultLoader.Value;

            return loader.ProcessLibrary(lib.id) != default;
        }
        
        private static string ListToStringStack(IEnumerable<Library> libs, string stopToken)
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

        private static string GetLibraryId(string libraryPath)
        {
            return Path.GetFullPath(libraryPath);
        }
        
        private static bool PathHasInvalidChars(string path)
        {
            return (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0);
        }
    }
}