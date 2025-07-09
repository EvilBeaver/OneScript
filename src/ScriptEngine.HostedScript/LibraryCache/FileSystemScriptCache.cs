using System;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using OneScript.Compilation;
using OneScript.Execution;

namespace ScriptEngine.HostedScript.LibraryCache
{
    public class FileSystemScriptCache : IScriptCacheStorage
    {
        private const string CACHE_EXTENSION = ".obj";
        private const string METADATA_EXTENSION = ".metadata.json";
        
        private readonly IModuleSerializer _serializer;

        public FileSystemScriptCache(IModuleSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Store(string key, IExecutableModule module)
        {
            var fileInfo = new FileInfo(key);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"Исходный файл не существует", key);
            }
            
            // Сериализуем модуль
            var cacheFile = GetCacheFilePath(key);
            try
            {
                using var stream = File.Create(cacheFile);
                _serializer.Serialize(module, stream);
            }
            catch (UnauthorizedAccessException)
            {
                LogOperation($"Нет прав для записи кэша в {cacheFile}. Кэширование отключено для данного расположения.");
                return;
            }
            catch (IOException ex)
            {
                LogOperation($"Ошибка ввода-вывода при записи кэша {cacheFile}: {ex.Message}. Кэширование отключено для данного расположения.");
                return;
            }
            
            var metadata = new CacheMetadata
            {
                SourceModifiedTime = fileInfo.LastWriteTime,
                SourceSize = fileInfo.Length,
                SourcePath = key,
                CacheCreatedTime = DateTime.UtcNow,
                RuntimeVersion = GetRuntimeVersion()
            };
            
            var metadataFile = GetMetadataFilePath(key);
            var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
            { 
                WriteIndented = false
            });
            
            try
            {
                File.WriteAllText(metadataFile, metadataJson);
            }
            catch (UnauthorizedAccessException)
            {
                LogOperation($"Нет прав для записи метаданных кэша в {metadataFile}. Кэширование отключено для данного расположения.");
            }
            catch (IOException ex)
            {
                LogOperation($"Ошибка ввода-вывода при записи метаданных {metadataFile}: {ex.Message}. Кэширование отключено для данного расположения.");
            }
            
            LogOperation($"Модуль успешно сохранен в кэш: {key}");
        }

        public IExecutableModule Load(string key)
        {
            var cacheFile = GetCacheFilePath(key);
            if (!File.Exists(cacheFile))
            {
                throw new ArgumentException($"Cache file {cacheFile} not found");
            }

            using var stream = File.OpenRead(cacheFile);
            return _serializer.Deserialize(stream);
        }

        public bool Exists(string key)
        {
            var metadataFile = GetMetadataFilePath(key);
            var dataFile = GetCacheFilePath(key);
            return File.Exists(metadataFile) && File.Exists(dataFile);
        }

        public bool IsValid(string key)
        {
            try
            {
                var sourceFileInfo = new FileInfo(key);
                if (!sourceFileInfo.Exists)
                    return false;

                if (!Exists(key))
                    return false;
                
                var metadataFile = GetMetadataFilePath(key);
                
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

        public void Delete(string key)
        {
            File.Delete(GetCacheFilePath(key));
            File.Delete(GetMetadataFilePath(key));
        }

        public bool CanStore(IExecutableModule module)
        {
            return _serializer.CanSerialize(module);
        }
        
        private void LogOperation(string message)
        {
            SystemLogger.Write(message);
        }
        
        private string GetRuntimeVersion()
        {
            return typeof(IExecutableModule).Assembly.GetName().Version!.ToString();
        }
        
        private string GetCacheFilePath(string sourceFile)
        {
            return sourceFile + CACHE_EXTENSION;
        }

        private string GetMetadataFilePath(string sourceFile)
        {
            return sourceFile + METADATA_EXTENSION;
        }
    }
}