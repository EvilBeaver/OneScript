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
using OneScript.Contexts;
using OneScript.Execution;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// Загрузчик бандлов — восстанавливает все модули из пакета
    /// </summary>
    public class BundleLoader
    {
        private readonly ScriptingEngine _engine;
        private readonly CompiledModulePackager _packager;
        private readonly Dictionary<string, IAttachableContext> _loadedModules;
        private string _bundlePath;

        public BundleLoader(ScriptingEngine engine)
        {
            _engine = engine;
            _packager = new CompiledModulePackager();
            _loadedModules = new Dictionary<string, IAttachableContext>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Загружает бандл из потока
        /// </summary>
        public LoadedBundle Load(Stream stream)
        {
            var package = MessagePackSerializer.Deserialize<CompiledPackageDto>(stream);
            return LoadPackage(package);
        }

        /// <summary>
        /// Загружает бандл из массива байт
        /// </summary>
        public LoadedBundle Load(byte[] data)
        {
            var package = MessagePackSerializer.Deserialize<CompiledPackageDto>(data);
            return LoadPackage(package);
        }

        /// <summary>
        /// Загружает бандл из файла
        /// </summary>
        public LoadedBundle LoadFromFile(string path)
        {
            _bundlePath = Path.GetFullPath(path);
            using (var stream = File.OpenRead(path))
            {
                return Load(stream);
            }
        }

        private LoadedBundle LoadPackage(CompiledPackageDto package)
        {
            ValidatePackage(package);

            var result = new LoadedBundle
            {
                Name = package.Name,
                Type = package.Type
            };

            // Загружаем скрипты в порядке LoadOrder
            var orderedScripts = package.Scripts
                .OrderBy(s => s.LoadOrder)
                .ToList();

            // Сначала загружаем все модули библиотек (без entry)
            foreach (var scriptDto in orderedScripts.Where(s => s.Type != ScriptType.Entry))
            {
                LoadLibraryScript(scriptDto, result);
            }

            // Теперь загружаем entry module (он может ссылаться на модули библиотек)
            var entryScript = orderedScripts.FirstOrDefault(s => s.Type == ScriptType.Entry);
            if (entryScript != null)
            {
                LoadEntryScript(entryScript, result);
            }

            return result;
        }

        private void LoadLibraryScript(PackagedScriptDto scriptDto, LoadedBundle result)
        {
            // Для модулей библиотек используем расширенный lookup, включающий уже загруженные модули
            var env = _engine.Environment;
            // Для бандла все модули ссылаются на .osc файл
            var module = _packager.ConvertFromDto(scriptDto.Module, env, _bundlePath);

            switch (scriptDto.Type)
            {
                case ScriptType.Module:
                    var instance = RegisterModule(scriptDto.Symbol, module);
                    if (instance != null)
                    {
                        result.Modules[scriptDto.Symbol] = module;
                        // Запоминаем для последующего использования в lookup
                        _loadedModules[scriptDto.Symbol] = instance;
                    }
                    break;

                case ScriptType.Class:
                    RegisterClass(scriptDto.Symbol, module);
                    result.Classes[scriptDto.Symbol] = module;
                    break;
            }
        }

        private void LoadEntryScript(PackagedScriptDto scriptDto, LoadedBundle result)
        {
            // Entry module загружается с учётом всех уже загруженных модулей
            var env = _engine.Environment;
            // Для бандла entry module тоже ссылается на .osc файл
            var module = _packager.ConvertFromDto(scriptDto.Module, env, _bundlePath);
            result.EntryModule = module;
        }

        private void ValidatePackage(CompiledPackageDto package)
        {
            if (package.MagicHeader != CompiledPackageDto.Magic)
            {
                throw new InvalidOperationException("Invalid compiled package format");
            }

            if (package.Version > CompiledPackageDto.FormatVersion)
            {
                throw new InvalidOperationException($"Unsupported package version: {package.Version}");
            }

            if (package.Type == PackageType.Library && package.Dependencies.Count > 0)
            {
                // Для библиотек с зависимостями нужно сначала загрузить зависимости
                // TODO: Реализовать для .oslib
                throw new NotSupportedException(
                    "Libraries with external dependencies are not yet supported. Use bundle format.");
            }
        }

        private UserScriptContextInstance RegisterModule(string symbol, StackRuntimeModule module)
        {
            if (string.IsNullOrEmpty(symbol))
                return null;

            var process = _engine.NewProcess();
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
            _engine.AttachedScriptsFactory?.RegisterTypeModule(symbol, module);
        }
    }

    /// <summary>
    /// Результат загрузки бандла
    /// </summary>
    public class LoadedBundle
    {
        public string Name { get; set; }
        public PackageType Type { get; set; }
        
        /// <summary>
        /// Главный модуль (точка входа)
        /// </summary>
        public StackRuntimeModule EntryModule { get; set; }
        
        /// <summary>
        /// Загруженные модули библиотек
        /// </summary>
        public Dictionary<string, StackRuntimeModule> Modules { get; } = 
            new Dictionary<string, StackRuntimeModule>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Загруженные классы библиотек
        /// </summary>
        public Dictionary<string, StackRuntimeModule> Classes { get; } = 
            new Dictionary<string, StackRuntimeModule>(StringComparer.OrdinalIgnoreCase);
    }
}
