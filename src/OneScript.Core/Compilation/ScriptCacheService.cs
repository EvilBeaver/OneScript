/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text.Json;
using OneScript.Execution;

namespace OneScript.Compilation
{
    /// <summary>
    /// Сервис кэширования скомпилированных сценариев
    /// </summary>
    public class ScriptCacheService : IScriptCacheService
    {
        private const string CACHE_EXTENSION = ".obj";
        private const string METADATA_EXTENSION = ".metadata.json";

        private IModuleSerializer _moduleSerializer;

        /// <summary>
        /// Включено ли кэширование
        /// </summary>
        public bool CachingEnabled { get; set; } = true;

        /// <summary>
        /// Событие для логирования операций кэша
        /// </summary>
        public event Action<string> CacheOperationLogged;

        /// <summary>
        /// Установить сериализатор модулей
        /// </summary>
        public void SetModuleSerializer(IModuleSerializer serializer)
        {
            _moduleSerializer = serializer;
        }

        public bool TryLoadFromCache(string sourceFile, out IExecutableModule module)
        {
            module = null;

            if (!CachingEnabled)
            {
                LogOperation($"Кэширование отключено для {sourceFile}");
                return false;
            }

            if (_moduleSerializer == null)
            {
                LogOperation($"Сериализатор модулей не установлен для {sourceFile}");
                return false;
            }

            try
            {
                if (!IsCacheValid(sourceFile))
                {
                    LogOperation($"Кэш недействителен для {sourceFile}");
                    return false;
                }

                var cacheFile = GetCacheFilePath(sourceFile);
                if (!File.Exists(cacheFile))
                {
                    LogOperation($"Файл кэша не найден: {cacheFile}");
                    return false;
                }

                // Загружаем сериализованный модуль
                using (var stream = File.OpenRead(cacheFile))
                {
                    module = _moduleSerializer.Deserialize(stream);
                }

                LogOperation($"Модуль успешно загружен из кэша: {sourceFile}");
                return true;
            }
            catch (Exception ex)
            {
                LogOperation($"Ошибка при загрузке из кэша {sourceFile}: {ex.Message}");
                return false;
            }
        }

        public void SaveToCache(string sourceFile, IExecutableModule module)
        {
            if (!CachingEnabled)
            {
                LogOperation($"Кэширование отключено, не сохраняем {sourceFile}");
                return;
            }

            if (_moduleSerializer == null || !_moduleSerializer.CanSerialize(module))
            {
                LogOperation($"Сериализатор недоступен или не поддерживает модуль {sourceFile}");
                return;
            }

            try
            {
                var fileInfo = new FileInfo(sourceFile);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException($"Исходный файл не существует: {sourceFile}", sourceFile);
                }

                var metadata = new CacheMetadata
                {
                    SourceModifiedTime = fileInfo.LastWriteTime,
                    SourceSize = fileInfo.Length,
                    SourcePath = sourceFile,
                    CacheCreatedTime = DateTime.UtcNow,
                    RuntimeVersion = GetRuntimeVersion()
                };

                var metadataFile = GetMetadataFilePath(sourceFile);
                var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                try
                {
                    File.WriteAllText(metadataFile, metadataJson);
                }
                catch (UnauthorizedAccessException)
                {
                    LogOperation($"Нет прав для записи метаданных кэша в {metadataFile}. Кэширование отключено для данного расположения.");
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    LogOperation($"Директория для кэша не найдена: {Path.GetDirectoryName(metadataFile)}. Кэширование отключено для данного расположения.");
                    return;
                }
                catch (IOException ex)
                {
                    LogOperation($"Ошибка ввода-вывода при записи метаданных {metadataFile}: {ex.Message}. Кэширование отключено для данного расположения.");
                    return;
                }

                // Сериализуем модуль
                var cacheFile = GetCacheFilePath(sourceFile);
                try
                {
                    using (var stream = File.Create(cacheFile))
                    {
                        _moduleSerializer.Serialize(module, stream);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    LogOperation($"Нет прав для записи кэша в {cacheFile}. Кэширование отключено для данного расположения.");
                    // Удаляем метаданные, если основной файл кэша не удалось создать
                    try { File.Delete(metadataFile); } catch { }
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    LogOperation($"Директория для кэша не найдена: {Path.GetDirectoryName(cacheFile)}. Кэширование отключено для данного расположения.");
                    // Удаляем метаданные, если основной файл кэша не удалось создать
                    try { File.Delete(metadataFile); } catch { }
                    return;
                }
                catch (IOException ex)
                {
                    LogOperation($"Ошибка ввода-вывода при записи кэша {cacheFile}: {ex.Message}. Кэширование отключено для данного расположения.");
                    // Удаляем метаданные, если основной файл кэша не удалось создать  
                    try { File.Delete(metadataFile); } catch { }
                    return;
                }

                LogOperation($"Модуль успешно сохранен в кэш: {sourceFile}");
            }
            catch (Exception ex)
            {
                LogOperation($"Ошибка сохранения кэша для {sourceFile}: {ex.Message}");
            }
        }

        public void ClearCache(string sourceFile)
        {
            try
            {
                var cacheFile = GetCacheFilePath(sourceFile);
                var metadataFile = GetMetadataFilePath(sourceFile);

                if (File.Exists(cacheFile))
                    File.Delete(cacheFile);

                if (File.Exists(metadataFile))
                    File.Delete(metadataFile);
            }
            catch (Exception)
            {
                // Игнорируем ошибки очистки кэша
            }
        }

        public bool IsCacheValid(string sourceFile)
        {
            if (!CachingEnabled)
                return false;

            try
            {
                var sourceFileInfo = new FileInfo(sourceFile);
                if (!sourceFileInfo.Exists)
                    return false;

                var metadataFile = GetMetadataFilePath(sourceFile);
                if (!File.Exists(metadataFile))
                    return false;

                var metadataJson = File.ReadAllText(metadataFile);
                var metadata = JsonSerializer.Deserialize<CacheMetadata>(metadataJson);

                // Проверяем, не изменился ли исходный файл
                if (sourceFileInfo.LastWriteTime != metadata.SourceModifiedTime ||
                    sourceFileInfo.Length != metadata.SourceSize)
                {
                    return false;
                }

                // Проверяем версию рантайма
                if (metadata.RuntimeVersion != GetRuntimeVersion())
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetCacheFilePath(string sourceFile)
        {
            return sourceFile + CACHE_EXTENSION;
        }

        private string GetMetadataFilePath(string sourceFile)
        {
            return sourceFile + METADATA_EXTENSION;
        }

        private string GetRuntimeVersion()
        {
            return typeof(IExecutableModule).Assembly.GetName().Version?.ToString() ?? "unknown";
        }

        private void LogOperation(string message)
        {
            CacheOperationLogged?.Invoke(message);
        }
    }
}