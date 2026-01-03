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
using MessagePack;
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Execution;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// Построитель скомпилированной библиотеки (.oslib)
    /// </summary>
    public class LibraryBuilder
    {
        private readonly ScriptingEngine _engine;
        private readonly ICompilerFrontend _compiler;
        private readonly CompiledModulePackager _packager;
        private readonly HashSet<string> _dependencies;
        private int _loadOrder;

        public LibraryBuilder(ScriptingEngine engine, ICompilerFrontend compiler)
        {
            _engine = engine;
            _compiler = compiler;
            _packager = new CompiledModulePackager();
            _dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _loadOrder = 0;
        }

        // Стандартные папки с модулями
        private static readonly string[] ModuleFolders = { "Modules", "Модули", "src" };
        
        // Стандартные папки с классами
        private static readonly string[] ClassFolders = { "Classes", "Классы" };
        
        // Папки, которые следует игнорировать
        private static readonly HashSet<string> IgnoredFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tests", "test", "тесты", "examples", "примеры", "doc", "docs", 
            "bin", "obj", ".git", ".svn", "node_modules", "addins"
        };

        /// <summary>
        /// Собирает библиотеку из папки
        /// </summary>
        public CompiledPackageDto Build(string libraryPath, IBslProcess process)
        {
            var libraryName = Path.GetFileName(libraryPath);

            var package = new CompiledPackageDto
            {
                Type = PackageType.Library,
                Name = libraryName
            };

            // Собираем модули из корня и стандартных папок
            CollectModules(libraryPath, libraryName, package, process);

            // Собираем классы из стандартных папок
            CollectClasses(libraryPath, libraryName, package, process);

            if (package.Scripts.Count == 0)
            {
                throw new InvalidOperationException($"No valid script files found in library: {libraryPath}");
            }

            // Добавляем собранные зависимости
            package.Dependencies.AddRange(_dependencies);

            return package;
        }

        private void CollectModules(string libraryPath, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            // Сначала ищем в корне библиотеки
            CompileModulesInFolder(libraryPath, libraryName, package, process);

            // Затем в стандартных папках модулей
            foreach (var folderName in ModuleFolders)
            {
                var modulesPath = Path.Combine(libraryPath, folderName);
                if (Directory.Exists(modulesPath))
                {
                    CompileModulesRecursive(modulesPath, libraryName, package, process);
                }
            }

            // Проверяем подпапки core, tools и т.д. (для библиотек типа oint)
            foreach (var subDir in Directory.EnumerateDirectories(libraryPath))
            {
                var dirName = Path.GetFileName(subDir);
                if (IgnoredFolders.Contains(dirName) || ClassFolders.Any(c => c.Equals(dirName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                // Проверяем есть ли внутри папка Modules
                foreach (var folderName in ModuleFolders)
                {
                    var nestedModulesPath = Path.Combine(subDir, folderName);
                    if (Directory.Exists(nestedModulesPath))
                    {
                        CompileModulesRecursive(nestedModulesPath, libraryName, package, process);
                    }
                }
            }
        }

        private void CollectClasses(string libraryPath, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            // Ищем классы в стандартных папках
            foreach (var folderName in ClassFolders)
            {
                var classesPath = Path.Combine(libraryPath, folderName);
                if (Directory.Exists(classesPath))
                {
                    CompileClassesRecursive(classesPath, libraryName, package, process);
                }
            }

            // Проверяем подпапки (для библиотек типа oint)
            foreach (var subDir in Directory.EnumerateDirectories(libraryPath))
            {
                var dirName = Path.GetFileName(subDir);
                if (IgnoredFolders.Contains(dirName))
                    continue;

                foreach (var folderName in ClassFolders)
                {
                    var nestedClassesPath = Path.Combine(subDir, folderName);
                    if (Directory.Exists(nestedClassesPath))
                    {
                        CompileClassesRecursive(nestedClassesPath, libraryName, package, process);
                    }
                }
            }
        }

        private void CompileModulesInFolder(string folderPath, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            var scriptFiles = Directory.EnumerateFiles(folderPath, "*.os")
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Path = x })
                .Where(x => Utils.IsValidIdentifier(x.Name))
                .ToList();

            foreach (var scriptFile in scriptFiles)
            {
                CompileModule(scriptFile.Path, scriptFile.Name, libraryName, package, process);
            }
        }

        private void CompileModulesRecursive(string folderPath, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            CompileModulesInFolder(folderPath, libraryName, package, process);

            // Рекурсивно обрабатываем подпапки
            foreach (var subDir in Directory.EnumerateDirectories(folderPath))
            {
                var dirName = Path.GetFileName(subDir);
                if (IgnoredFolders.Contains(dirName))
                    continue;

                // Пропускаем папки классов внутри папок модулей
                if (ClassFolders.Any(c => c.Equals(dirName, StringComparison.OrdinalIgnoreCase)))
                {
                    CompileClassesRecursive(subDir, libraryName, package, process);
                }
                else
                {
                    CompileModulesRecursive(subDir, libraryName, package, process);
                }
            }
        }

        private void CompileModule(string filePath, string moduleName, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            // Проверяем, не добавлен ли уже модуль с таким именем
            if (package.Scripts.Any(s => s.Type == ScriptType.Module && 
                s.Symbol.Equals(moduleName, StringComparison.OrdinalIgnoreCase)))
            {
                return; // Модуль уже добавлен
            }

            var source = _engine.Loader.FromFile(filePath);
            CollectDependencies(source);

            var module = _compiler.Compile(source, process);

            if (!(module is StackRuntimeModule stackModule))
            {
                throw new InvalidOperationException(
                    $"Only stack runtime modules can be compiled to library. " +
                    $"File '{filePath}' uses native runtime.");
            }

            var scriptDto = new PackagedScriptDto
            {
                Type = ScriptType.Module,
                Symbol = moduleName,
                OwnerLibrary = libraryName,
                LoadOrder = _loadOrder++,
                Module = _packager.ConvertToDto(stackModule)
            };

            package.Scripts.Add(scriptDto);
        }

        private void CompileClassesRecursive(string classesPath, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            var classFiles = Directory.EnumerateFiles(classesPath, "*.os")
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Path = x })
                .Where(x => Utils.IsValidIdentifier(x.Name))
                .ToList();

            foreach (var classFile in classFiles)
            {
                CompileClass(classFile.Path, classFile.Name, libraryName, package, process);
            }

            // Рекурсивно обрабатываем подпапки
            foreach (var subDir in Directory.EnumerateDirectories(classesPath))
            {
                var dirName = Path.GetFileName(subDir);
                if (IgnoredFolders.Contains(dirName))
                    continue;

                CompileClassesRecursive(subDir, libraryName, package, process);
            }
        }

        private void CompileClass(string filePath, string className, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            // Проверяем, не добавлен ли уже класс с таким именем
            if (package.Scripts.Any(s => s.Type == ScriptType.Class &&
                s.Symbol.Equals(className, StringComparison.OrdinalIgnoreCase)))
            {
                return; // Класс уже добавлен
            }

            var source = _engine.Loader.FromFile(filePath);
            CollectDependencies(source);

            var module = _compiler.Compile(source, process);

            if (!(module is StackRuntimeModule stackModule))
            {
                throw new InvalidOperationException(
                    $"Only stack runtime modules can be compiled to library. " +
                    $"File '{filePath}' uses native runtime.");
            }

            var scriptDto = new PackagedScriptDto
            {
                Type = ScriptType.Class,
                Symbol = className,
                OwnerLibrary = libraryName,
                LoadOrder = _loadOrder++,
                Module = _packager.ConvertToDto(stackModule)
            };

            package.Scripts.Add(scriptDto);
        }

        private void CollectDependencies(SourceCode source)
        {
            // Парсим исходник для поиска директив #Использовать
            // Зависимости добавляются в _dependencies
            // 
            // Примечание: это упрощённая реализация.
            // В идеале нужно использовать лексер/парсер для точного извлечения.

            var code = source.GetSourceCode();
            var lines = code.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("#Использовать", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("#Use", StringComparison.OrdinalIgnoreCase))
                {
                    var depName = ExtractDependencyName(trimmed);
                    if (!string.IsNullOrEmpty(depName) && !depName.StartsWith("\"") && !depName.StartsWith("."))
                    {
                        // Это ссылка на внешнюю библиотеку по имени
                        _dependencies.Add(depName);
                    }
                }
            }
        }

        private string ExtractDependencyName(string directive)
        {
            // #Использовать ИмяБиблиотеки
            // #Использовать "путь/к/библиотеке"
            var parts = directive.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var name = parts[1].Trim();
                // Убираем комментарии
                var commentIdx = name.IndexOf("//");
                if (commentIdx >= 0)
                {
                    name = name.Substring(0, commentIdx).Trim();
                }
                return name;
            }
            return null;
        }

        /// <summary>
        /// Сохраняет библиотеку в поток
        /// </summary>
        public void Save(Stream stream, CompiledPackageDto package)
        {
            MessagePackSerializer.Serialize(stream, package);
        }

        /// <summary>
        /// Сохраняет библиотеку в массив байт
        /// </summary>
        public byte[] Save(CompiledPackageDto package)
        {
            return MessagePackSerializer.Serialize(package);
        }
    }
}
