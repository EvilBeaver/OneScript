/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using FluentAssertions;
using OneScript.StandardLibrary.Timezones;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class TimeZoneConverterTests
    {
        [Fact]
        public void Kiev_Summer_Dst_ToUtc_And_Back()
        {
            var dtLocal = new DateTime(2025, 6, 24, 10, 0, 0);
            var utc = TimeZoneConverter.ToUniversalTime(dtLocal, "Europe/Kiev");
            utc.Should().Be(new DateTime(2025, 6, 24, 7, 0, 0, DateTimeKind.Utc));

            var backLocal = TimeZoneConverter.ToLocalTime(utc, "Europe/Kiev");
            backLocal.Should().Be(dtLocal);
        }

        [Fact]
        public void Kiev_EET_StandardTimeOffset_Is_BaseUtcOffset_2h()
        {
            var dtLocal = new DateTime(2025, 6, 24, 10, 0, 0);
            var offsetSeconds = TimeZoneConverter.StandardTimeOffset("EET", dtLocal);
            offsetSeconds.Should().Be(7200);
        }

        [Fact]
        public void Kiev_EET_TimeZonePresentation_Is_GmtPlus02()
        {
            var presentation = TimeZoneConverter.TimeZonePresentation("EET");
            presentation.Should().Be("GMT+02:00");
        }
    }
}