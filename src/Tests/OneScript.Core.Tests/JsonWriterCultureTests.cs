/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Globalization;
using FluentAssertions;
using OneScript.StandardLibrary.Json;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class JsonWriterCultureTests
    {
        [Fact]
        public void ExponentFormatUsesDotWithCommaDecimalCulture()
        {
            var culture = CultureInfo.CurrentCulture;
            // Культура с запятой в дробной части, как в русской локали, независимо от машины
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            try
            {
                var writer = new JSONWriter();
                writer.SetString();
                writer.WriteStartArray();
                writer.WriteValue(ValueFactory.Create(1000), true);
                writer.WriteValue(ValueFactory.Create(1.5m), true);
                writer.WriteEndArray();

                writer.Close().Should().ContainAll("1.000000E+003", "1.500000E+000");
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
            }
        }
    }
}
