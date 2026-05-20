/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using FluentAssertions;
using OneScript.BinaryData;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class BinaryDataMemoryLimitConfigurationTests
    {
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
            var warnings = new List<string>();

            var bytes = BinaryDataMemoryLimitConfiguration.ResolveFromConfigString(rawValue, warnings.Add);

            bytes.Should().Be(expectedBytes);
            warnings.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UsesDefaultWhenValueIsMissing(string rawValue)
        {
            var bytes = BinaryDataMemoryLimitConfiguration.ResolveFromConfigString(rawValue, _ => { });

            bytes.Should().Be(BinaryDataConfigurationDefaults.InMemoryMaxBytes);
        }

        [Theory]
        [InlineData("512x")]
        [InlineData("m512")]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("2g")]
        public void UsesDefaultForInvalidValue(string rawValue)
        {
            var warnings = new List<string>();

            var bytes = BinaryDataMemoryLimitConfiguration.ResolveFromConfigString(rawValue, warnings.Add);

            bytes.Should().Be(BinaryDataConfigurationDefaults.InMemoryMaxBytes);
            warnings.Should().NotBeEmpty();
        }
    }
}
