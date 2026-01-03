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

            // Находим все .os файлы в папке библиотеки
            var scriptFiles = Directory.EnumerateFiles(libraryPath, "*.os")
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Path = x })
                .Where(x => Utils.IsValidIdentifier(x.Name))
                .ToList();

            if (scriptFiles.Count == 0)
            {
                throw new InvalidOperationException($"No valid script files found in library: {libraryPath}");
            }

            // Компилируем каждый модуль
            foreach (var scriptFile in scriptFiles)
            {
                var source = _engine.Loader.FromFile(scriptFile.Path);

                // Собираем зависимости из директив #Использовать
                CollectDependencies(source);

                // Компилируем модуль
                var module = _compiler.Compile(source, process);

                if (!(module is StackRuntimeModule stackModule))
                {
                    throw new InvalidOperationException(
                        $"Only stack runtime modules can be compiled to library. " +
                        $"File '{scriptFile.Path}' uses native runtime.");
                }

                var scriptDto = new PackagedScriptDto
                {
                    Type = ScriptType.Module,
                    Symbol = scriptFile.Name,
                    OwnerLibrary = libraryName,
                    LoadOrder = _loadOrder++,
                    Module = _packager.ConvertToDto(stackModule)
                };

                package.Scripts.Add(scriptDto);
            }

            // Проверяем папку Классы
            var classesPath = Path.Combine(libraryPath, "Классы");
            if (Directory.Exists(classesPath))
            {
                CompileClasses(classesPath, libraryName, package, process);
            }

            // Альтернативное имя папки
            classesPath = Path.Combine(libraryPath, "Classes");
            if (Directory.Exists(classesPath))
            {
                CompileClasses(classesPath, libraryName, package, process);
            }

            // Добавляем собранные зависимости
            package.Dependencies.AddRange(_dependencies);

            return package;
        }

        private void CompileClasses(string classesPath, string libraryName, CompiledPackageDto package, IBslProcess process)
        {
            var classFiles = Directory.EnumerateFiles(classesPath, "*.os")
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Path = x })
                .Where(x => Utils.IsValidIdentifier(x.Name))
                .ToList();

            foreach (var classFile in classFiles)
            {
                var source = _engine.Loader.FromFile(classFile.Path);
                CollectDependencies(source);

                var module = _compiler.Compile(source, process);

                if (!(module is StackRuntimeModule stackModule))
                {
                    throw new InvalidOperationException(
                        $"Only stack runtime modules can be compiled to library. " +
                        $"File '{classFile.Path}' uses native runtime.");
                }

                var scriptDto = new PackagedScriptDto
                {
                    Type = ScriptType.Class,
                    Symbol = classFile.Name,
                    OwnerLibrary = libraryName,
                    LoadOrder = _loadOrder++,
                    Module = _packager.ConvertToDto(stackModule)
                };

                package.Scripts.Add(scriptDto);
            }
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
