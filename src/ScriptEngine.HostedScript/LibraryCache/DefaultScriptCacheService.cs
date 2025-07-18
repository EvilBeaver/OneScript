/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using OneScript.Compilation;
using OneScript.Execution;

namespace ScriptEngine.HostedScript.LibraryCache
{
    /// <summary>
    /// Сервис кэширования скомпилированных сценариев
    /// </summary>
    public class DefaultScriptCacheService : IScriptCacheService
    {
        private readonly IScriptCacheStorage _cacheStorage;
        
        public DefaultScriptCacheService(
            IScriptCacheStorage cacheStorage,
            OneScriptLibraryOptions libOptions)
        {
            _cacheStorage = cacheStorage;
            CachingEnabled = libOptions.ScriptCachingEnabled;
        }

        /// <summary>
        /// Включено ли кэширование
        /// </summary>
        private bool CachingEnabled { get; }

        public bool TryLoadFromCache(string location, out IExecutableModule module)
        {
            module = null;

            if (!CachingEnabled)
            {
                LogOperation($"Кэширование отключено для {location}");
                return false;
            }
            
            try
            {
                if (!_cacheStorage.IsValid(location))
                {
                    LogOperation($"Кэш недействителен для {location}");
                    return false;
                }

                try
                {
                    module = _cacheStorage.Load(location);
                }
                catch (SerializationException)
                {
                    _cacheStorage.Delete(location);
                    return false;
                }
                
                LogOperation($"Модуль успешно загружен из кэша: {location}");
                return true;
            }
            catch (Exception ex)
            {
                LogOperation($"Ошибка при загрузке из кэша {location}: {ex.Message}");
                return false;
            }
        }

        public void SaveToCache(IExecutableModule module)
        {
            var sourceFile = module.Source.Location;
            if (!CachingEnabled)
            {
                LogOperation($"Кэширование отключено, не сохраняем {sourceFile}");
                return;
            }

            if (!_cacheStorage.CanStore(module))
            {
                LogOperation($"Сериализатор недоступен или не поддерживает модуль {sourceFile}");
                return;
            }

            try
            {
                _cacheStorage.Store(module.Source.Location, module);
            }
            catch (Exception ex)
            {
                LogOperation($"Ошибка сохранения кэша для {sourceFile}: {ex.Message}");
            }
        }

        private void LogOperation(string message)
        {
            SystemLogger.Write(message);
        }
    }
}