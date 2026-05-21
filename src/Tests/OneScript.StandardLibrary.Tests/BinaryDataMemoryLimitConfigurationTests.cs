/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using FluentAssertions;
using Moq;
using OneScript.StandardLibrary.Binary;
using ScriptEngine;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    [Collection("SystemLogger")]
    public class BinaryDataMemoryLimitConfigurationTests
    {
        List<string> _messages = new List<string>();
        
        public BinaryDataMemoryLimitConfigurationTests()
        {
            var mock = new Mock<ISystemLogWriter>();
            mock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(str => _messages.Add(str));
            
            SystemLogger.SetWriter(mock.Object);
        }

        [Theory]
        [InlineData("52428800", 52428800)]
        [InlineData("512k", 512 * 1024)]
        [InlineData("512K", 512 * 1024)]
        [InlineData("50m", 50 * 1024 * 1024)]
        [InlineData("50M", 50 * 1024 * 1024)]
        [InlineData("1g", 1024 * 1024 * 1024)]
        [InlineData("1G", 1024 * 1024 * 1024)]
        public void ResolvesByteSizeWithOptionalSuffix(string rawValue, int expectedBytes)
        {
            var bytes = MockConfig(rawValue).MaxBytesInMemory;

            bytes.Should().Be(expectedBytes);
            _messages.Should().BeEmpty();
        }

        private static IBinaryDataMemoryLimit MockConfig(string rawValue)
        {
            var kvStore = new KeyValueConfig();
            kvStore.Merge(new Dictionary<string, string>
            {
                {BinaryDataOptions.IN_MEMORY_LIMIT_KEY_NAME, rawValue}
            }, Mock.Of<IConfigProvider>());
            
            return new BinaryDataOptions(kvStore);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UsesDefaultWhenValueIsMissing(string rawValue)
        {
            var bytes = MockConfig(rawValue).MaxBytesInMemory;

            bytes.Should().Be(BinaryDataConstants.DEFAULT_IN_MEMORY_LIMIT);
        }

        [Theory]
        [InlineData("512x")]
        [InlineData("m512")]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("2g")]
        public void UsesDefaultForInvalidValue(string rawValue)
        {
            var bytes = MockConfig(rawValue).MaxBytesInMemory;

            bytes.Should().Be(BinaryDataConstants.DEFAULT_IN_MEMORY_LIMIT);
            _messages.Should().NotBeEmpty();
        }

        [Fact]
        public void TestMagicMaxValue()
        {
            MockConfig(BinaryDataOptions.IN_MEMORY_MAX_MAGIC).MaxBytesInMemory.Should().Be(BinaryDataConstants.SYSTEM_IN_MEMORY_LIMIT);
        }
    }
}
