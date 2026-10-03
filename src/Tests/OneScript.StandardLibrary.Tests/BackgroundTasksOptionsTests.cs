/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using FluentAssertions;
using Moq;
using OneScript.StandardLibrary.Tasks;
using ScriptEngine;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    [Collection("SystemLogger")]
    public class BackgroundTasksOptionsTests
    {
        private readonly List<string> _messages = new List<string>();

        public BackgroundTasksOptionsTests()
        {
            var mock = new Mock<ISystemLogWriter>();
            mock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(str => _messages.Add(str));

            SystemLogger.SetWriter(mock.Object);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("5000", 5000)]
        [InlineData(" 200 ", 200)]
        public void ReadsCapacity(string rawValue, int expected)
        {
            MockConfig(rawValue).Capacity.Should().Be(expected);
            _messages.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void UsesDefaultWhenNotSet(string rawValue)
        {
            MockConfig(rawValue).Capacity.Should().Be(BackgroundTasksManager.DefaultCapacity);
            _messages.Should().BeEmpty();
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("много")]
        [InlineData("1k")]
        public void WarnsAndUsesDefaultOnInvalidValue(string rawValue)
        {
            MockConfig(rawValue).Capacity.Should().Be(BackgroundTasksManager.DefaultCapacity);
            _messages.Should().ContainSingle()
                .Which.Should().Contain(BackgroundTasksOptions.CAPACITY_KEY_NAME);
        }

        private static BackgroundTasksOptions MockConfig(string rawValue)
        {
            var kvStore = new KeyValueConfig();
            kvStore.Merge(new Dictionary<string, string>
            {
                {BackgroundTasksOptions.CAPACITY_KEY_NAME, rawValue}
            }, Mock.Of<IConfigProvider>());

            return new BackgroundTasksOptions(kvStore);
        }
    }
}
