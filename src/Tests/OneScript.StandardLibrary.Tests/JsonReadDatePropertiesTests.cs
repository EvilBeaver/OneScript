/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Exceptions;
using OneScript.Localization;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Json;
using OneScript.Values;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class JsonReadDatePropertiesTests
    {
        private const string JsonText =
            "{\"a\":\"2026-02-01T15:30:45\"," +
            "\"b\":\"2026-02-01T15:30:45.123\"," +
            "\"c\":\"2026-02-01T15:30:45Z\"," +
            "\"d\":\"2026-02-01T15:30:45+03:00\"," +
            "\"e\":\"2026-02-01T15:30:45.1234567Z\"}";

        private static DateTime LocalOf(int h, int m, int s) =>
            TimeZoneInfo.ConvertTimeFromUtc(
                new DateTime(2026, 2, 1, h, m, s, DateTimeKind.Utc),
                TimeZoneInfo.Local);

        private static IValue GetStructureProperty(StructureImpl structure, string name) =>
            structure.GetPropValue(structure.GetPropertyNumber(name));

        private static IValue GetMapProperty(MapImpl map, string name) =>
            map.GetIndexedValue(ValueFactory.Create(name));

        private static void AssertDateWithoutFraction(IValue value, DateTime expected)
        {
            Assert.IsType<BslDateValue>(value);
            var date = (DateTime)(BslDateValue)value;
            Assert.Equal(expected, date);
            Assert.Equal(0, date.Ticks % TimeSpan.TicksPerSecond);
        }

        private static ArrayImpl CreateNameArray(params string[] names)
        {
            var array = new ArrayImpl();
            foreach (var name in names)
                array.Add(ValueFactory.Create(name));
            return array;
        }

        [Fact]
        public void ReadJson_Without_Date_Property_Names_Keeps_Strings()
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
        public void ReadJson_With_Single_String_Name_Parses_Only_That_Property()
        {
            var reader = new JSONReader();
            reader.SetString(JsonText);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: ValueFactory.Create("a"));

            AssertDateWithoutFraction(GetStructureProperty(data, "a"), new DateTime(2026, 2, 1, 15, 30, 45));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "b"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "c"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "d"));
            Assert.IsType<BslStringValue>(GetStructureProperty(data, "e"));
        }

        [Fact]
        public void ReadJson_With_ArrayImpl_Names_Parses_Iso_Dates()
        {
            var names = CreateNameArray("a", "b", "c", "d", "e");

            var reader = new JSONReader();
            reader.SetString(JsonText);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: names,
                ExpectedDateFormat: JSONDateFormatEnum.ISO);

            AssertDateWithoutFraction(GetStructureProperty(data, "a"), new DateTime(2026, 2, 1, 15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "b"), new DateTime(2026, 2, 1, 15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "c"), LocalOf(15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "d"), LocalOf(12, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "e"), LocalOf(15, 30, 45));
        }

        [Fact]
        public void ReadJson_With_FixedArrayImpl_Names_Parses_Iso_Dates()
        {
            var names = new FixedArrayImpl(CreateNameArray("a", "b", "c", "d", "e"));

            var reader = new JSONReader();
            reader.SetString(JsonText);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: names,
                ExpectedDateFormat: JSONDateFormatEnum.ISO);

            AssertDateWithoutFraction(GetStructureProperty(data, "a"), new DateTime(2026, 2, 1, 15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "b"), new DateTime(2026, 2, 1, 15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "c"), LocalOf(15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "d"), LocalOf(12, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(data, "e"), LocalOf(15, 30, 45));
        }

        [Fact]
        public void ReadJson_With_Names_Parses_Dates_At_Any_Nesting_Level()
        {
            const string nestedJson =
                "{\"outer\":{\"a\":\"2026-02-01T15:30:45Z\"}," +
                "\"items\":[{\"b\":\"2026-02-01T15:30:45Z\"}]}";

            var names = CreateNameArray("a", "b");

            var reader = new JSONReader();
            reader.SetString(nestedJson);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: names);

            var outer = (StructureImpl)GetStructureProperty(data, "outer");
            var items = (ArrayImpl)GetStructureProperty(data, "items");
            var item = (StructureImpl)items.GetIndexedValue(ValueFactory.Create(0));

            AssertDateWithoutFraction(GetStructureProperty(outer, "a"), LocalOf(15, 30, 45));
            AssertDateWithoutFraction(GetStructureProperty(item, "b"), LocalOf(15, 30, 45));
        }

        [Fact]
        public void ReadJson_ToMap_With_Names_Parses_Dates()
        {
            var names = CreateNameArray("a");

            var reader = new JSONReader();
            reader.SetString("{\"a\":\"2026-02-01T15:30:45Z\"}");
            var data = (MapImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                ReadToMap: true,
                PropertiesWithDateValuesNames: names);

            AssertDateWithoutFraction(GetMapProperty(data, "a"), LocalOf(15, 30, 45));
        }

        [Fact]
        public void ReadJson_With_Names_Does_Not_Convert_NonString_Values()
        {
            const string json =
                "{\"a\":123,\"b\":null,\"c\":{\"x\":1},\"d\":[1,2]}";

            var names = CreateNameArray("a", "b", "c", "d", "missing");

            var reader = new JSONReader();
            reader.SetString(json);
            var data = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: names);

            Assert.IsType<BslNumericValue>(GetStructureProperty(data, "a"));
            Assert.IsType<BslUndefinedValue>(GetStructureProperty(data, "b"));
            Assert.IsType<StructureImpl>(GetStructureProperty(data, "c"));
            Assert.IsType<ArrayImpl>(GetStructureProperty(data, "d"));
        }

        [Fact]
        public void ReadJsonDate_Drops_Fractional_Seconds_For_Iso()
        {
            var result = new GlobalJsonFunctions().ReadJSONDate("2026-02-01T15:30:45.123", JSONDateFormatEnum.ISO);
            var date = (DateTime)(BslDateValue)result;

            Assert.Equal(new DateTime(2026, 2, 1, 15, 30, 45), date);
            Assert.Equal(0, date.Ticks % TimeSpan.TicksPerSecond);
        }

        [Fact]
        public void ReadJsonDate_Converts_Z_And_Offset_To_Local()
        {
            var fromZ = (DateTime)(BslDateValue)new GlobalJsonFunctions().ReadJSONDate(
                "2026-02-01T15:30:45Z",
                JSONDateFormatEnum.ISO);
            var fromOffset = (DateTime)(BslDateValue)new GlobalJsonFunctions().ReadJSONDate(
                "2026-02-01T15:30:45+03:00",
                JSONDateFormatEnum.ISO);

            Assert.Equal(LocalOf(15, 30, 45), fromZ);
            Assert.Equal(LocalOf(12, 30, 45), fromOffset);
        }

        [Fact]
        public void ReadJson_With_Invalid_Date_String_Throws_RuntimeException()
        {
            var names = CreateNameArray("a");

            var reader = new JSONReader();
            reader.SetString("{\"a\":\"not-a-date\"}");

            var exception = Assert.Throws<RuntimeException>(() =>
                new GlobalJsonFunctions().ReadJSON(reader, PropertiesWithDateValuesNames: names));

            Assert.True(exception.ErrorDescription.BilingualEquals(
                "Представление даты имеет неверный формат.",
                "Invalid date presentation format"));
        }

        [Fact]
        public void ReadJsonDate_With_Invalid_String_Throws_RuntimeException()
        {
            var exception = Assert.Throws<RuntimeException>(() =>
                new GlobalJsonFunctions().ReadJSONDate("not-a-date", JSONDateFormatEnum.ISO));

            Assert.True(exception.ErrorDescription.BilingualEquals(
                "Представление даты имеет неверный формат.",
                "Invalid date presentation format"));
        }

        [Fact]
        public void ReadJson_With_JavaScript_Format_Throws()
        {
            var reader = new JSONReader();
            reader.SetString(JsonText);

            Assert.Throws<RuntimeException>(() =>
                new GlobalJsonFunctions().ReadJSON(
                    reader,
                    PropertiesWithDateValuesNames: CreateNameArray("a"),
                    ExpectedDateFormat: JSONDateFormatEnum.JavaScript));
        }

        [Fact]
        public void ReadJsonDate_With_JavaScript_Format_Throws()
        {
            Assert.Throws<RuntimeException>(() =>
                new GlobalJsonFunctions().ReadJSONDate("2026-02-01T15:30:45", JSONDateFormatEnum.JavaScript));
        }

        [Fact]
        public void ReadJson_With_Empty_Names_Keeps_Strings()
        {
            var reader = new JSONReader();
            reader.SetString(JsonText);

            var withEmptyArray = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: new ArrayImpl());

            reader.SetString(JsonText);
            var withNull = (StructureImpl)new GlobalJsonFunctions().ReadJSON(
                reader,
                PropertiesWithDateValuesNames: null);

            foreach (var data in new[] { withEmptyArray, withNull })
            {
                Assert.IsType<BslStringValue>(GetStructureProperty(data, "a"));
                Assert.IsType<BslStringValue>(GetStructureProperty(data, "b"));
            }
        }
    }
}
