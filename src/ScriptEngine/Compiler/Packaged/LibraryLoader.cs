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
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.Execution;
using ScriptEngine.Libraries;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// Загрузчик скомпилированных библиотек (.oslib)
    /// </summary>
    public class LibraryLoader
    {
        private readonly ScriptingEngine _engine;
        private readonly CompiledModulePackager _packager;

        public LibraryLoader(ScriptingEngine engine)
        {
            _engine = engine;
            _packager = new CompiledModulePackager();
        }

        /// <summary>
        /// Загружает библиотеку из потока
        /// </summary>
        public LoadedLibrary Load(Stream stream, IBslProcess process)
        {
            var package = MessagePackSerializer.Deserialize<CompiledPackageDto>(stream);
            return LoadPackage(package, process);
        }

        /// <summary>
        /// Загружает библиотеку из файла
        /// </summary>
        public LoadedLibrary LoadFromFile(string path, IBslProcess process)
        {
            using (var stream = File.OpenRead(path))
            {
                return Load(stream, process);
            }
        }

        private LoadedLibrary LoadPackage(CompiledPackageDto package, IBslProcess process)
        {
            ValidatePackage(package);

            var result = new LoadedLibrary
            {
                Name = package.Name,
                Dependencies = package.Dependencies.ToList()
            };

            var env = _engine.Environment;

            // Загружаем скрипты в порядке LoadOrder
            var orderedScripts = package.Scripts
                .OrderBy(s => s.LoadOrder)
                .ToList();

            // Сначала загружаем модули
            foreach (var scriptDto in orderedScripts.Where(s => s.Type == ScriptType.Module))
            {
                var module = _packager.ConvertFromDto(scriptDto.Module, env);
                var instance = RegisterModule(scriptDto.Symbol, module, process);
                if (instance != null)
                {
                    result.Modules[scriptDto.Symbol] = module;
                }
            }

            // Затем классы
            foreach (var scriptDto in orderedScripts.Where(s => s.Type == ScriptType.Class))
            {
                var module = _packager.ConvertFromDto(scriptDto.Module, env);
                RegisterClass(scriptDto.Symbol, module);
                result.Classes[scriptDto.Symbol] = module;
            }

            return result;
        }

        private void ValidatePackage(CompiledPackageDto package)
        {
            if (package.MagicHeader != CompiledPackageDto.Magic)
            {
                throw new InvalidOperationException("Invalid compiled library format");
            }

            if (package.Version > CompiledPackageDto.FormatVersion)
            {
                throw new InvalidOperationException($"Unsupported library version: {package.Version}");
            }

            if (package.Type != PackageType.Library)
            {
                throw new InvalidOperationException($"Expected library package, got: {package.Type}");
            }
        }

        private UserScriptContextInstance RegisterModule(string symbol, StackRuntimeModule module, IBslProcess process)
        {
            if (string.IsNullOrEmpty(symbol))
                return null;

            var instance = _engine.CreateUninitializedSDO(module);

            // Регистрируем как глобальное свойство
            _engine.Environment.InjectGlobalProperty(instance, symbol, symbol, true);

            // Инициализируем модуль
            _engine.InitializeSDO(instance, process);

            return instance;
        }

        private void RegisterClass(string symbol, StackRuntimeModule module)
        {
            if (string.IsNullOrEmpty(symbol))
                return;

            // Регистрируем класс через фабрику
            var factory = _engine.AttachedScriptsFactory;
            if (factory == null)
            {
                throw new InvalidOperationException($"AttachedScriptsFactory is not initialized. Cannot register class '{symbol}'");
            }
            
            factory.RegisterTypeModule(symbol, module);
        }
    }

    /// <summary>
    /// Результат загрузки библиотеки
    /// </summary>
    public class LoadedLibrary
    {
        public string Name { get; set; }

        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// Загруженные модули библиотеки
        /// </summary>
        public Dictionary<string, StackRuntimeModule> Modules { get; } =
            new Dictionary<string, StackRuntimeModule>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Загруженные классы библиотеки
        /// </summary>
        public Dictionary<string, StackRuntimeModule> Classes { get; } =
            new Dictionary<string, StackRuntimeModule>(StringComparer.OrdinalIgnoreCase);
    }
}
