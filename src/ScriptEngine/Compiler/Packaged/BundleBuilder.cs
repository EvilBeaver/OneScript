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
using OneScript.Sources;
using ScriptEngine.Libraries;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// Построитель бандла — собирает все зависимости в один пакет
    /// </summary>
    public class BundleBuilder
    {
        private readonly ScriptingEngine _engine;
        private readonly ICompilerFrontend _compiler;
        private readonly CompiledModulePackager _packager;
        private readonly HashSet<string> _collectedSources;
        private int _loadOrder;

        public BundleBuilder(ScriptingEngine engine, ICompilerFrontend compiler)
        {
            _engine = engine;
            _compiler = compiler;
            _packager = new CompiledModulePackager();
            _collectedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _loadOrder = 0;
        }

        /// <summary>
        /// Собирает бандл из главного скрипта
        /// </summary>
        public CompiledPackageDto Build(SourceCode entrySource, IBslProcess process)
        {
            var package = new CompiledPackageDto
            {
                Type = PackageType.Bundle,
                Name = Path.GetFileNameWithoutExtension(entrySource.Location ?? "bundle")
            };

            // Компилируем главный модуль — это запустит загрузку всех зависимостей
            var entryModule = _compiler.Compile(entrySource, process);

            if (!(entryModule is StackRuntimeModule stackModule))
            {
                throw new InvalidOperationException(
                    "Only stack runtime modules can be bundled. Native modules are not supported.");
            }

            // Собираем все загруженные модули библиотек из окружения
            // и строим маппинг контекстов на символьные имена
            var contextSymbols = new Dictionary<IAttachableContext, string>();
            CollectLibraryModules(package, contextSymbols);

            // Передаём маппинг в packager для правильной сериализации bindings
            _packager.SetContextSymbols(contextSymbols);

            // Добавляем главный модуль последним (он зависит от библиотек)
            var entryDto = new PackagedScriptDto
            {
                Type = ScriptType.Entry,
                Symbol = null,
                OwnerLibrary = null,
                LoadOrder = _loadOrder++,
                Module = _packager.ConvertToDto(stackModule)
            };
            package.Scripts.Add(entryDto);

            return package;
        }

        /// <summary>
        /// Сохраняет бандл в поток
        /// </summary>
        public void Save(Stream stream, CompiledPackageDto package)
        {
            MessagePackSerializer.Serialize(stream, package);
        }

        /// <summary>
        /// Сохраняет бандл в массив байт
        /// </summary>
        public byte[] Save(CompiledPackageDto package)
        {
            return MessagePackSerializer.Serialize(package);
        }

        private void CollectLibraryModules(CompiledPackageDto package, Dictionary<IAttachableContext, string> contextSymbols)
        {
            var env = _engine.Environment;

            // Проходим по всем присоединённым контекстам
            foreach (var context in env.AttachedContexts)
            {
                // Ищем UserScriptContextInstance — это загруженные модули библиотек
                if (context is UserScriptContextInstance userScript)
                {
                    TryCollectUserScript(userScript, package, null, contextSymbols);
                }
                
                // PropertyBag содержит глобальные свойства, включая модули
                if (context is PropertyBag propertyBag)
                {
                    CollectFromPropertyBag(propertyBag, package, contextSymbols);
                }
            }
        }

        private void CollectFromPropertyBag(PropertyBag propertyBag, CompiledPackageDto package, Dictionary<IAttachableContext, string> contextSymbols)
        {
            for (int i = 0; i < propertyBag.Count; i++)
            {
                var value = propertyBag.GetPropValue(i);
                if (value is UserScriptContextInstance userScript)
                {
                    var symbol = propertyBag.GetPropName(i);
                    TryCollectUserScript(userScript, package, symbol, contextSymbols);
                }
            }
        }

        private void TryCollectUserScript(
            UserScriptContextInstance userScript, 
            CompiledPackageDto package,
            string knownSymbol,
            Dictionary<IAttachableContext, string> contextSymbols)
        {
            var module = userScript.Module;
            if (!(module is StackRuntimeModule stackModule))
                return;

            var sourceLocation = stackModule.Source?.Location;
            
            // Пропускаем если уже собрали (по пути к исходнику)
            if (!string.IsNullOrEmpty(sourceLocation))
            {
                if (_collectedSources.Contains(sourceLocation))
                    return;
                _collectedSources.Add(sourceLocation);
            }

            // Определяем символьное имя
            var symbol = knownSymbol ?? FindModuleSymbol(userScript);

            // Добавляем в маппинг контекстов
            if (!string.IsNullOrEmpty(symbol))
            {
                contextSymbols[userScript] = symbol;
            }

            var scriptDto = new PackagedScriptDto
            {
                Type = ScriptType.Module,
                Symbol = symbol,
                OwnerLibrary = null, // TODO: определить библиотеку-владельца
                LoadOrder = _loadOrder++,
                Module = _packager.ConvertToDto(stackModule)
            };

            package.Scripts.Add(scriptDto);
        }

        private string FindModuleSymbol(UserScriptContextInstance userScript)
        {
            var env = _engine.Environment;

            // Ищем в присоединённых контекстах
            foreach (var context in env.AttachedContexts)
            {
                if (context is PropertyBag propertyBag)
                {
                    for (int i = 0; i < propertyBag.Count; i++)
                    {
                        var value = propertyBag.GetPropValue(i);
                        if (ReferenceEquals(value, userScript))
                        {
                            return propertyBag.GetPropName(i);
                        }
                    }
                }
            }

            return null;
        }
    }
}
