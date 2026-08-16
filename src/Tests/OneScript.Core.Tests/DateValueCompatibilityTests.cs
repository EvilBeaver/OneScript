/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Native.Runtime;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Json;
using OneScript.StandardLibrary.Timezones;
using OneScript.StandardLibrary.Xml;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class DateValueCompatibilityTests
    {
        private static readonly BslDateValue Base =
            (BslDateValue)ValueFactory.Create(new DateTime(2026, 2, 1, 15, 30, 45));

        private const string JsonText =
            "{\"a\":\"2026-02-01T15:30:45\"," +
            "\"b\":\"2026-02-01T15:30:45.123\"," +
            "\"c\":\"2026-02-01T15:30:45Z\"," +
            "\"d\":\"2026-02-01T15:30:45+03:00\"," +
            "\"e\":\"2026-02-01T15:30:45.1234567Z\"}";

        private static DateTime DropFraction(DateTime dt) =>
            new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

        private static IValue GetStructureProperty(StructureImpl structure, string name) =>
            structure.GetPropValue(structure.GetPropertyNumber(name));

        public static IEnumerable<object[]> FractionalSubtractionCases => new[]
        {
            new object[] { 0.5m, 0.5m },
            new object[] { 1.5m, 1.5m },
            new object[] { 0.25m, 0.25m },
            new object[] { 2m, 2m },
        };

        private static JObject ParseJson(string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json))
            {
                DateParseHandling = DateParseHandling.None
            })
            {
                return JObject.Load(reader);
            }
        }

        private static string WriteStructureToJson(StructureImpl structure)
        {
            var writer = new JSONWriter();
            writer.SetString();
            new GlobalJsonFunctions().WriteJSON(ForbiddenBslProcess.Instance, writer, structure);
            return writer.Close();
        }

        [Theory]
        [InlineData(0.001)]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(0.999)]
        [InlineData(0.0001)]
        public void Date_Addition_Above_1C_Precision_Changes_Value(double delta)
        {
            var result = ValueFactory.Add(Base, ValueFactory.Create((decimal)delta), ForbiddenBslProcess.Instance);

            Assert.False(Base.Equals(result));
        }

        [Theory]
        [InlineData(0.00001)]
        [InlineData(0.0000001)]
        [InlineData(0.00004)]
        public void Date_Addition_Below_Half_Of_1C_Precision_Is_Ignored(double delta)
        {
            var result = ValueFactory.Add(Base, ValueFactory.Create((decimal)delta), ForbiddenBslProcess.Instance);

            Assert.True(Base.Equals(result));
        }

        [Theory]
        [InlineData(0.00005, 0.0001)]
        [InlineData(0.00006, 0.0001)]
        [InlineData(0.00016, 0.0002)]
        [InlineData(0.00025, 0.0003)]
        public void Date_Addition_Is_Rounded_To_1C_Precision(double addition, double expectedShift)
        {
            var result = ValueFactory.Add(Base, ValueFactory.Create((decimal)addition), ForbiddenBslProcess.Instance);
            var expectedDelta = (decimal)expectedShift;

            Assert.Equal(expectedDelta, ValueFactory.Sub(result, Base).AsNumber());

            var expectedDate = ValueFactory.Add(Base, ValueFactory.Create(expectedDelta), ForbiddenBslProcess.Instance);
            Assert.True(result.Equals(expectedDate));
        }

        [Fact]
        public void Date_Fractional_Addition_Is_Not_Rounded_To_Whole_Second()
        {
            var plus06 = ValueFactory.Add(Base, ValueFactory.Create(0.6m), ForbiddenBslProcess.Instance);
            var plus1 = ValueFactory.Add(Base, ValueFactory.Create(1m), ForbiddenBslProcess.Instance);
            var plus04 = ValueFactory.Add(Base, ValueFactory.Create(0.4m), ForbiddenBslProcess.Instance);
            var plus0004 = ValueFactory.Add(Base, ValueFactory.Create(0.0004m), ForbiddenBslProcess.Instance);
            var plus0006 = ValueFactory.Add(Base, ValueFactory.Create(0.0006m), ForbiddenBslProcess.Instance);

            Assert.False(Base.Equals(plus06));
            Assert.False(plus1.Equals(plus06));
            Assert.False(Base.Equals(plus04));
            Assert.False(Base.Equals(plus0004));
            Assert.False(Base.Equals(plus0006));
        }

        [Theory]
        [MemberData(nameof(FractionalSubtractionCases))]
        public void Date_Subtraction_Returns_Fractional_Seconds(decimal delta, decimal expectedDiff)
        {
            var shifted = ValueFactory.Add(Base, ValueFactory.Create(delta), ForbiddenBslProcess.Instance);
            var diff = ValueFactory.Sub(shifted, Base).AsNumber();

            Assert.Equal(expectedDiff, diff);
        }

        [Fact]
        public void Date_Subtraction_Result_Is_Not_Integer()
        {
            var diff = ValueFactory.Sub(
                ValueFactory.Add(Base, ValueFactory.Create(0.5m), ForbiddenBslProcess.Instance),
                Base).AsNumber();

            Assert.NotEqual(decimal.Truncate(diff), diff);
        }

        [Fact]
        public void Date_Comparison_Distinguishes_Fractions()
        {
            var fractional = ValueFactory.Add(Base, ValueFactory.Create(0.001m), ForbiddenBslProcess.Instance);

            Assert.True(fractional.CompareTo(Base) > 0);
            Assert.True(fractional.CompareTo(Base) >= 0);
            Assert.False(fractional.CompareTo(Base) < 0);
            Assert.False(Base.Equals(fractional));
            Assert.NotEqual(fractional, Base);
        }

        [Fact]
        public void CurrentDate_Has_No_Fractional_Seconds()
        {
            var current = BuiltInFunctions.CurrentDate();

            Assert.Equal(DropFraction(current), current);
        }

        [Fact]
        public void CurrentUniversalDate_Has_No_Fractional_Seconds()
        {
            var universal = (DateTime)(BslDateValue)new StandardGlobalContext().CurrentUniversalDate();

            Assert.Equal(DropFraction(universal), universal);
        }

        [Fact]
        public void ToLocalTime_Of_Whole_Second_Date_Has_No_Fractional_Seconds()
        {
            var utc = DropFraction(DateTime.UtcNow);
            var local = TimeZoneConverter.ToLocalTime(utc);

            Assert.Equal(DropFraction(local), local);
        }

        [Fact]
        public void Timezone_Roundtrip_Preserves_Fractional_Seconds()
        {
            var frac = (BslDateValue)ValueFactory.Add(Base, ValueFactory.Create(0.001m), ForbiddenBslProcess.Instance);
            var utc = TimeZoneConverter.ToUniversalTime((DateTime)frac);
            var back = TimeZoneConverter.ToLocalTime(utc);

            Assert.True(frac.Equals((BslValue)ValueFactory.Create(back)));
        }

        [Fact]
        public void Date_ToString_Hides_Fractional_Seconds()
        {
            var shifted = ValueFactory.Add(Base, ValueFactory.Create(0.123m), ForbiddenBslProcess.Instance);

            Assert.Equal(Base.ToString(), shifted.ToString());
        }

        [Fact]
        public void Format_Ignores_Fractional_Seconds()
        {
            var shifted = ValueFactory.Add(Base, ValueFactory.Create(0.123m), ForbiddenBslProcess.Instance);

            Assert.Equal("45", ValueFormatter.Format((BslValue)shifted, "ДФ=сс"));
            Assert.Equal("20260201153045", ValueFormatter.Format((BslValue)shifted, "ДФ=ггггММддЧЧммсс"));
        }

        [Fact]
        public void XmlString_Drops_Fractional_Seconds()
        {
            var shifted = ValueFactory.Add(Base, ValueFactory.Create(0.123m), ForbiddenBslProcess.Instance);
            var mgr = new Mock<IGlobalsManager>();
            mgr.Setup(m => m.GetInstance(It.IsAny<Type>())).Returns((IAttachableContext)null);
            var xml = (XmlGlobalFunctions)XmlGlobalFunctions.CreateInstance(mgr.Object);

            Assert.Equal("2026-02-01T15:30:45", xml.XMLString((BslValue)shifted));
        }

        [Fact]
        public void Json_Write_Naive_Date_Uses_Iso_Seconds()
        {
            var structure = new StructureImpl();
            structure.Insert("Литерал", ValueFactory.Create(new DateTime(2026, 2, 1, 15, 30, 45)));
            structure.Insert("ПустаяДата", ValueFactory.Create(DateTime.MinValue));

            var json = WriteStructureToJson(structure);
            var obj = ParseJson(json);

            Assert.Equal("2026-02-01T15:30:45", obj["Литерал"]?.ToString());
            Assert.Equal("0001-01-01T00:00:00", obj["ПустаяДата"]?.ToString());
        }

        [Fact]
        public void Json_Write_Drops_Fractional_Seconds()
        {
            var structure = new StructureImpl();
            structure.Insert("Дробная", ValueFactory.Add(Base, ValueFactory.Create(0.123m), ForbiddenBslProcess.Instance));

            var json = WriteStructureToJson(structure);
            var obj = ParseJson(json);

            Assert.Equal("2026-02-01T15:30:45", obj["Дробная"]?.ToString());
        }

        [Fact]
        public void Json_Write_Ignores_DateTime_Kind()
        {
            var structure = new StructureImpl();
            structure.Insert("Local", ValueFactory.Create(DropFraction(DateTime.Now)));
            structure.Insert("Utc", ValueFactory.Create(DropFraction(DateTime.UtcNow)));

            var json = WriteStructureToJson(structure);
            var obj = ParseJson(json);
            var pattern = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$");

            var localValue = obj["Local"]?.ToString();
            var utcValue = obj["Utc"]?.ToString();

            Assert.Matches(pattern, localValue);
            Assert.Matches(pattern, utcValue);
            Assert.DoesNotContain("Z", utcValue);
            Assert.DoesNotContain("+", utcValue);
            Assert.DoesNotContain("-", utcValue.Substring(10));
        }

        [Fact]
        public void Json_Read_Without_Date_Property_Names_Keeps_Strings()
        {
            var reader = new JSONReader();
            reader.SetString(JsonText);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(reader);

            Assert.IsType<BslStringValue>(GetStructureProperty(data, "a"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "b"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "c"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "d"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "e"));

            Assert.Equal("2026-02-01T15:30:45", GetStructureProperty(data, "a").ToString());
            Assert.Equal("2026-02-01T15:30:45.123", GetStructureProperty(data, "b").ToString());
            Assert.Equal("2026-02-01T15:30:45Z", GetStructureProperty(data, "c").ToString());
            Assert.Equal("2026-02-01T15:30:45+03:00", GetStructureProperty(data, "d").ToString());
            Assert.Equal("2026-02-01T15:30:45.1234567Z", GetStructureProperty(data, "e").ToString());
        }

        [Fact]
        public void Json_Read_With_Date_Property_Names_Parses_Iso()
        {
            var names = new ArrayImpl();
            names.Add(ValueFactory.Create("a"));
            names.Add(ValueFactory.Create("b"));
            names.Add(ValueFactory.Create("c"));
            names.Add(ValueFactory.Create("d"));
            names.Add(ValueFactory.Create("e"));

            var reader = new JSONReader();
            reader.SetString(JsonText);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                ReadToMap: false,
                PropertiesWithDateValuesNames: names,
                ExpectedDateFormat: null);

            var expectedLocalFromUtc = TimeZoneInfo.ConvertTimeFromUtc(
                new DateTime(2026, 2, 1, 15, 30, 45, DateTimeKind.Utc),
                TimeZoneInfo.Local);
            var expectedLocalFromUtcD = TimeZoneInfo.ConvertTimeFromUtc(
                new DateTime(2026, 2, 1, 12, 30, 45, DateTimeKind.Utc),
                TimeZoneInfo.Local);

            AssertDateWithoutFraction(GetStructureProperty(data, "a"), new DateTime(2026, 2, 1, 15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "b"), new DateTime(2026, 2, 1, 15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "c"), expectedLocalFromUtc);
            AssertDateWithoutFraction(GetStructureProperty(data, "d"), expectedLocalFromUtcD);
            AssertDateWithoutFraction(GetStructureProperty(data, "e"), expectedLocalFromUtc);
        }

        [Fact]
        public void ReadJsonDate_Drops_Fractional_Seconds()
        {
            var result = new GlobalJsonFunctions().ReadJSONDate("2026-02-01T15:30:45.123", JSONDateFormatEnum.ISO);
            var date = (DateTime)(BslDateValue)result;

            Assert.Equal(new DateTime(2026, 2, 1, 15, 30, 45), date);
            Assert.Equal(0, date.Ticks % TimeSpan.TicksPerSecond);
        }

        private static void AssertDateWithoutFraction(IValue value, DateTime expected)
        {
            Assert.IsType<BslDateValue>(value);
            var date = (DateTime)(BslDateValue)value;
            Assert.Equal(expected, date);
            Assert.Equal(0, date.Ticks % TimeSpan.TicksPerSecond);
        }
    }
}
