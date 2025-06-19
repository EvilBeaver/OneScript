/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using Xunit;
using FluentAssertions;
using OneScript.Compilation;
using OneScript.Execution;
using Moq;

namespace OneScript.Core.Tests
{
    public class ScriptCacheServiceTests : IDisposable
    {
        private readonly string _testScriptPath;
        private readonly ScriptCacheService _cacheService;
        private readonly Mock<IModuleSerializer> _mockSerializer;
        private readonly Mock<IExecutableModule> _mockModule;

        public ScriptCacheServiceTests()
        {
            _testScriptPath = Path.GetTempFileName();
            File.WriteAllText(_testScriptPath, "// Тестовый сценарий\nСообщить(\"Привет, мир!\");");
            
            _cacheService = new ScriptCacheService();
            
            // Настроим mock сериализатор
            _mockSerializer = new Mock<IModuleSerializer>();
            _mockModule = new Mock<IExecutableModule>();
            
            _mockSerializer.Setup(s => s.CanSerialize(It.IsAny<IExecutableModule>())).Returns(true);
            _mockSerializer.Setup(s => s.Serialize(It.IsAny<IExecutableModule>(), It.IsAny<Stream>()))
                .Callback<IExecutableModule, Stream>((module, stream) =>
                {
                    // Записываем простые тестовые данные
                    var writer = new BinaryWriter(stream);
                    writer.Write("test-module-data");
                });
            _mockSerializer.Setup(s => s.Deserialize(It.IsAny<Stream>())).Returns(_mockModule.Object);
            
            _cacheService.SetModuleSerializer(_mockSerializer.Object);
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_testScriptPath))
                    File.Delete(_testScriptPath);

                var cacheFile = _testScriptPath + ".obj";
                var metadataFile = _testScriptPath + ".metadata.json";

                if (File.Exists(cacheFile))
                    File.Delete(cacheFile);

                if (File.Exists(metadataFile))
                    File.Delete(metadataFile);
            }
            catch
            {
                // Игнорируем ошибки очистки
            }
        }

        [Fact]
        public void IsCacheValid_NewFile_ReturnsFalse()
        {
            // Arrange & Act
            var result = _cacheService.IsCacheValid(_testScriptPath);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void SaveToCache_CreatesMetadataFile()
        {
            // Arrange & Act
            _cacheService.SaveToCache(_testScriptPath, _mockModule.Object);

            // Assert
            var metadataFile = _testScriptPath + ".metadata.json";
            File.Exists(metadataFile).Should().BeTrue();
        }

        [Fact]
        public void SaveToCache_CreatesObjFile()
        {
            // Arrange & Act
            _cacheService.SaveToCache(_testScriptPath, _mockModule.Object);

            // Assert
            var cacheFile = _testScriptPath + ".obj";
            File.Exists(cacheFile).Should().BeTrue();
        }

        [Fact]
        public void IsCacheValid_AfterSave_ReturnsTrue()
        {
            // Arrange
            _cacheService.SaveToCache(_testScriptPath, _mockModule.Object);

            // Act
            var result = _cacheService.IsCacheValid(_testScriptPath);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsCacheValid_AfterFileModification_ReturnsFalse()
        {
            // Arrange
            _cacheService.SaveToCache(_testScriptPath, _mockModule.Object);
            
            // Симулируем изменение файла
            System.Threading.Thread.Sleep(1); // Гарантируем другое время модификации
            File.AppendAllText(_testScriptPath, "\n// Изменение");

            // Act
            var result = _cacheService.IsCacheValid(_testScriptPath);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ClearCache_RemovesCacheFiles()
        {
            // Arrange
            _cacheService.SaveToCache(_testScriptPath, _mockModule.Object);
            var cacheFile = _testScriptPath + ".obj";
            var metadataFile = _testScriptPath + ".metadata.json";

            // Act
            _cacheService.ClearCache(_testScriptPath);

            // Assert
            File.Exists(cacheFile).Should().BeFalse();
            File.Exists(metadataFile).Should().BeFalse();
        }

        [Fact]
        public void CachingEnabled_WhenDisabled_DoesNotCreateFiles()
        {
            // Arrange
            _cacheService.CachingEnabled = false;

            // Act
            _cacheService.SaveToCache(_testScriptPath, _mockModule.Object);

            // Assert
            var cacheFile = _testScriptPath + ".obj";
            var metadataFile = _testScriptPath + ".metadata.json";
            
            File.Exists(cacheFile).Should().BeFalse();
            File.Exists(metadataFile).Should().BeFalse();
        }

        [Fact]
        public void TryLoadFromCache_WhenCachingDisabled_ReturnsFalse()
        {
            // Arrange
            _cacheService.CachingEnabled = false;

            // Act
            var result = _cacheService.TryLoadFromCache(_testScriptPath, out var module);

            // Assert
            result.Should().BeFalse();
            module.Should().BeNull();
        }
    }
}