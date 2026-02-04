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
using OneScript.Compilation;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Localization;
using OneScript.Sources;

namespace ScriptEngine.HostedScript
{
    public class FileSystemDependencyResolver : IDependencyResolver
    {
        public const string PREDEFINED_LOADER_FILE = "package-loader.os";
        private readonly List<Library> _libs = new List<Library>();
        private LibraryLoader _defaultLoader;
        private object _defaultLoaderLocker = new object();
        private string _libraryRoot;

        #region Private classes

        private class Library
        {
            public string id;
            public ProcessingState state;
            public LibraryLoader customLoader;
            public PackageInfo loadingResult;
        }

        private enum ProcessingState
        {
            Discovered,
            Processed
        }

        private enum LoadStatus
        {
            NotFound,   // Каталог библиотеки не существует
            Empty,      // Каталог найден, но библиотека не содержит исполняемых файлов
            Success     // Библиотека успешно загружена
        }

        private class LoadResult
        {
            public PackageInfo Package { get; set; }
            public LoadStatus Status { get; set; }
            
            public static LoadResult NotFound() => new LoadResult { Status = LoadStatus.NotFound };
            public static LoadResult Empty() => new LoadResult { Status = LoadStatus.Empty };
            public static LoadResult Success(PackageInfo package) => new LoadResult 
            { 
                Status = LoadStatus.Success, 
                Package = package 
            };
        }

        #endregion

        public FileSystemDependencyResolver()
        {
        }
        
        public IList<string> SearchDirectories { get;} = new List<string>();

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
                if (string.IsNullOrWhiteSpace(value))
                {
                    _libraryRoot = null;
                }
                
                _libraryRoot = value;
            }
        }
            

        private ScriptingEngine Engine { get; set; }
        
        public void Initialize(ScriptingEngine engine)
        {
            Engine = engine;
        }
        
        public PackageInfo Resolve(SourceCode module, string libraryName, IBslProcess process)
        {
            bool quoted = PrepareQuoted(ref libraryName);

            var result = quoted ?
                LoadByRelativePath(module, libraryName, process) :
                LoadByName(libraryName, process);

            if (result.Status == LoadStatus.NotFound)
            {
                throw new CompilerException($"Библиотека не найдена: '{libraryName}'");
            }
            else if (result.Status == LoadStatus.Empty)
            {
                throw new CompilerException(
                    $"Библиотека '{libraryName}' найдена, но не содержит исполняемых файлов.\n" +
                    "Для получения подробной информации установите переменную окружения OS_LIBRARY_LOADER_TRACE=1");
            }

            return result.Package;
        }

        private LoadResult LoadByName(string libraryName, IBslProcess process)
        {
            foreach (var path in SearchDirectories)
            {
                if(!Directory.Exists(path))
                    continue;

                var libraryPath = Path.Combine(path, libraryName);
                var result = LoadByPath(libraryPath, process);
                
                // Если библиотека найдена (успешно загружена или пуста), сразу возвращаем
                // Не ищем дальше, так как это более приоритетный путь
                if (result.Status != LoadStatus.NotFound)
                    return result;
            }

            // Если в SearchDirectories ничего не нашли, проверяем rootPath
            var rootPath = Path.Combine(LibraryRoot, libraryName);
            return LoadByPath(rootPath, process);
        }

        private LoadResult LoadByRelativePath(SourceCode module, string libraryPath, IBslProcess process)
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

            return LoadByPath(realPath, process);
        }

        private LibraryLoader CreateDefaultLoader(IBslProcess process)
        {
            var loaderscript = Path.Combine(LibraryRoot, PREDEFINED_LOADER_FILE);
            return File.Exists(loaderscript) ? 
                LibraryLoader.Create(Engine, loaderscript, process) 
                : LibraryLoader.Create(Engine, process);
        }

        private LibraryLoader GetDefaultLoader(IBslProcess process)
        {
            if (_defaultLoader != default)
                return _defaultLoader;
            
            lock (_defaultLoaderLocker)
            {
                if (_defaultLoader == default)
                {
                    _defaultLoader = CreateDefaultLoader(process);
                }
            }

            return _defaultLoader;
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

        private LoadResult LoadByPath(string libraryPath, IBslProcess process)
        {
            if (!Directory.Exists(libraryPath))
                return LoadResult.NotFound();

            var package = LoadLibraryInternal(libraryPath, process);
            
            return package == null 
                ? LoadResult.Empty() 
                : LoadResult.Success(package);
        }
        
        private PackageInfo LoadLibraryInternal(string libraryPath, IBslProcess process)
        {
            var id = GetLibraryId(libraryPath);
            var existedLib = _libs.FirstOrDefault(x => x.id == id);
            if(existedLib != null)
            {
                if (existedLib.state == ProcessingState.Discovered)
                {
                    string libStack = ListToStringStack(_libs, id);
                    throw new DependencyResolveException(
                        new BilingualString(
                            $"Ошибка загрузки библиотеки {id}. Обнаружены циклические зависимости.\n",
                            $"Error loading library {id}. Circular dependencies found.\n") + libStack);
                }
                
                return existedLib.loadingResult;
            }

            var newLib = new Library() { id = id, state = ProcessingState.Discovered };
            int newLibIndex = _libs.Count;
            
            var customLoaderFile = Path.Combine(libraryPath, PREDEFINED_LOADER_FILE);
            if (File.Exists(customLoaderFile))
                newLib.customLoader = LibraryLoader.Create(Engine, customLoaderFile, process);

            PackageInfo package;
            try
            {
                _libs.Add(newLib);
                package = ProcessLibrary(newLib, process);
                newLib.state = ProcessingState.Processed;
                newLib.loadingResult = package;
            }
            catch (Exception)
            {
                _libs.RemoveAt(newLibIndex);
                throw;
            }

            return package;
        }

        private PackageInfo ProcessLibrary(Library lib, IBslProcess process)
        {
            LibraryLoader loader;
            if (lib.customLoader != null)
                loader = lib.customLoader;
            else
                loader = GetDefaultLoader(process);

            return loader.ProcessLibrary(lib.id, process);
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
                offset += "  ";
            }

            return builder.ToString();
        }

        private static string GetLibraryId(string libraryPath)
        {
            return Path.GetFullPath(libraryPath);
        }
        
        private static bool PathHasInvalidChars(string path)
        {
            return (!string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0);
        }
    }
}
