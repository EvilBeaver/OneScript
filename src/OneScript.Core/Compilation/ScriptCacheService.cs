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

        /// <summary>
        /// Включено ли кэширование
        /// </summary>
        public bool CachingEnabled { get; set; } = true;

        public bool TryLoadFromCache(string sourceFile, out IExecutableModule module)
        {
            module = null;

            if (!CachingEnabled)
                return false;

            try
            {
                if (!IsCacheValid(sourceFile))
                    return false;

                var cacheFile = GetCacheFilePath(sourceFile);
                if (!File.Exists(cacheFile))
                    return false;

                // Для простоты сейчас не будем загружать из кэша,
                // так как это требует глубокой сериализации IExecutableModule
                // Пока просто возвращаем false - реализация кэшированной загрузки
                // потребует дополнительной работы с сериализацией
                return false;
            }
            catch (Exception)
            {
                // В случае ошибки загрузки из кэша, компилируем заново
                return false;
            }
        }

        public void SaveToCache(string sourceFile, IExecutableModule module)
        {
            if (!CachingEnabled)
                return;

            try
            {
                var fileInfo = new FileInfo(sourceFile);
                if (!fileInfo.Exists)
                    return;

                var metadata = new CacheMetadata
                {
                    SourceModifiedTime = fileInfo.LastWriteTime,
                    SourceSize = fileInfo.Length,
                    SourcePath = sourceFile,
                    CacheCreatedTime = DateTime.Now,
                    RuntimeVersion = GetRuntimeVersion()
                };

                var metadataFile = GetMetadataFilePath(sourceFile);
                var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(metadataFile, metadataJson);

                // Создаем пустой .obj файл как маркер того, что кэш был создан
                // Полная реализация сериализации IExecutableModule потребует больше времени
                var cacheFile = GetCacheFilePath(sourceFile);
                File.WriteAllText(cacheFile, ""); // пустой файл-маркер
            }
            catch (Exception)
            {
                // Ошибки кэширования не должны прерывать работу
                // Просто игнорируем и продолжаем без кэша
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
    }
}