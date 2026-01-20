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

namespace OneScript.StandardLibrary.Tests
{
    public class NullCharacterHandlingTests
    {
        [Fact]
        public void FileContext_WithNullCharacter_ShouldStripNullCharacter()
        {
            // Arrange
            var testDir = Path.GetTempPath();
            var testFileName = "test.txt";
            var testPath = Path.Combine(testDir, testFileName);
            
            // Create a test file
            File.WriteAllText(testPath, "test content");
            
            try
            {
                // Act - path with null character should be handled without exception
                var fileWithNull = new FileContext(testPath + "\0");
                
                // Assert
                fileWithNull.FullName.Should().Be(Path.GetFullPath(testPath));
                fileWithNull.Name.Should().Be(testFileName);
                fileWithNull.Exists().Should().BeTrue();
            }
            finally
            {
                // Cleanup
                if (File.Exists(testPath))
                    File.Delete(testPath);
            }
        }
        
        [Fact]
        public void FileContext_WithMultipleNullCharacters_ShouldStripAllNullCharacters()
        {
            // Arrange
            var testDir = Path.GetTempPath();
            var testFileName = "test2.txt";
            var testPath = Path.Combine(testDir, testFileName);
            
            // Create a test file
            File.WriteAllText(testPath, "test content");
            
            try
            {
                // Act - path with multiple null characters
                var pathWithNulls = testPath.Replace("test", "te\0st\0");
                var fileWithNull = new FileContext(pathWithNulls);
                
                // Assert
                fileWithNull.FullName.Should().Be(Path.GetFullPath(testPath));
            }
            finally
            {
                // Cleanup
                if (File.Exists(testPath))
                    File.Delete(testPath);
            }
        }
        
        [Fact]
        public void FileContext_WithNullCharacterInDirectoryPath_ShouldWork()
        {
            // Arrange & Act - directory path with null character should not throw
            var dirPath = Path.GetTempPath() + "\0";
            var file = new FileContext(dirPath);
            
            // Assert - should not throw exception
            file.FullName.Should().Be(Path.GetFullPath(Path.GetTempPath()));
        }
        
        [Fact]
        public void FindFiles_WithNullCharacterInPath_ShouldNotThrow()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var testSubDir = Path.Combine(tempDir, "NullCharTest_" + Guid.NewGuid());
            Directory.CreateDirectory(testSubDir);
            
            // Create a test file
            var testFile = Path.Combine(testSubDir, "test.txt");
            File.WriteAllText(testFile, "test content");
            
            try
            {
                var fileOps = new FileOperations();
                
                // Act - FindFiles with null character in directory path
                var pathWithNull = testSubDir + "\0";
                var result = fileOps.FindFiles(pathWithNull, "*.txt");
                
                // Assert
                result.Should().NotBeNull();
                result.Count().Should().Be(1);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testSubDir))
                    Directory.Delete(testSubDir, true);
            }
        }
        
        [Fact]
        public void FindFiles_WithNullCharacterInMask_ShouldNotThrow()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var testSubDir = Path.Combine(tempDir, "NullCharTest2_" + Guid.NewGuid());
            Directory.CreateDirectory(testSubDir);
            
            // Create a test file
            var testFile = Path.Combine(testSubDir, "test.txt");
            File.WriteAllText(testFile, "test content");
            
            try
            {
                var fileOps = new FileOperations();
                
                // Act - FindFiles with null character in mask
                var maskWithNull = "*.txt\0";
                var result = fileOps.FindFiles(testSubDir, maskWithNull);
                
                // Assert
                result.Should().NotBeNull();
                result.Count().Should().Be(1);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testSubDir))
                    Directory.Delete(testSubDir, true);
            }
        }
    }
}
