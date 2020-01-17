using System;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ValuesTest
    {
        [Fact]
        public void BooleanEquality()
        {
            Assert.True(BooleanValue.True.AsBoolean());
            Assert.False(BooleanValue.False.AsBoolean());

            Assert.Same(BooleanValue.True, ValueFactory.Create(true));
            Assert.Same(BooleanValue.False, ValueFactory.Create(false));
            Assert.NotEqual(BooleanValue.False, BooleanValue.True);
            Assert.Equal(0, BooleanValue.False.AsNumber());
            Assert.Equal(1, BooleanValue.True.AsNumber());

            Assert.True(BooleanValue.True.CompareTo(BooleanValue.False) > 0);

            Assert.Throws<RuntimeException>(() => BooleanValue.True.AsDate());
            Assert.Throws<RuntimeException>(() => BooleanValue.True.AsObject());
        }

        [Theory]
        [InlineData("ru", "Да", "Нет")]
        [InlineData("en", "True", "False")]
        [InlineData("jp", "True", "False")]
        public void BooleanStringLocales(string locale, string trueString, string falseString)
        {
            Locale.SystemLanguageISOName = locale;
            Assert.Equal(trueString, BooleanValue.True.AsString());
            Assert.Equal(falseString, BooleanValue.False.AsString());
        }

        [Fact]
        public void NumbersEquality()
        {
            var num1 = NumberValue.Create(12.5);
            var num2 = NumberValue.Create(12.5);
            var num3 = NumberValue.Create(7);
            var num4 = NumberValue.Create(0);

            Assert.Equal(num1, num2);
            Assert.True(num1.Equals(num2));
            Assert.NotEqual(num1, num3);

            Assert.Equal(0, num4.AsNumber());
            Assert.Equal(7, num3.AsNumber());

            Assert.True(num1.AsBoolean());
            Assert.True(num2.AsBoolean());
            Assert.True(num3.AsBoolean());
            Assert.False(num4.AsBoolean());

            Assert.True(num1.CompareTo(num2) == 0);
            Assert.True(num1.CompareTo(num3) > 0);
            Assert.True(num4.CompareTo(num3) < 0);

            Assert.Equal("12.5", num1.AsString());
            Assert.Throws<RuntimeException>(() => num1.AsDate());
            Assert.Throws<RuntimeException>(() => num1.AsObject());
        }

        [Fact]
        public void PopularNumbersReferenceEquality()
        {
            for (int i = 0; i < 10; i++)
            {
                var n1 = ValueFactory.Create(i);
                var n2 = ValueFactory.Create(i);

                Assert.Same(n1, n2);
            }
        }

        [Fact]
        public void StringValueTests()
        {
            var trueString = ValueFactory.Create("ИстИНа");
            Assert.True(trueString.DataType == DataType.String);
            Assert.True(trueString.AsBoolean());
            Assert.True(trueString.AsString() == "ИстИНа");

            var falseString = ValueFactory.Create("лОжЬ");
            Assert.False(falseString.AsBoolean());
            Assert.True(falseString.AsString() == "лОжЬ");

            var dateString = ValueFactory.Create("20140101");
            DateTime jan_01_14 = new DateTime(2014,01,01);
            Assert.True(dateString.AsDate() == jan_01_14);

            var numString = ValueFactory.Create("012.12");
            Assert.True(numString.AsNumber() == 12.12m);

            Assert.Throws<RuntimeException>(() => dateString.AsObject());
            Assert.Throws<RuntimeException>(() => trueString.AsNumber());
        }
        
        [Fact]
        public void Undefined_Value_Test()
        {
            var value = ValueFactory.Create();
            Assert.True(value.DataType == DataType.Undefined);
            Assert.True(value.AsString() == "");

            Assert.Throws<RuntimeException>(() => value.AsNumber());
            Assert.Throws<RuntimeException>(() => value.AsBoolean());
            Assert.Throws<RuntimeException>(() => value.AsObject());
            Assert.Throws<RuntimeException>(() => value.AsDate());
        }

        [Fact]
        public void Null_Value_Test()
        {
            var value = ValueFactory.CreateNullValue();
            Assert.True(value.DataType == DataType.GenericValue);
            Assert.True(value.AsString() == "");

            Assert.Throws<RuntimeException>(() => value.AsNumber());
            Assert.Throws<RuntimeException>(() => value.AsBoolean());
            Assert.Throws<RuntimeException>(() => value.AsObject());
            Assert.Throws<RuntimeException>(() => value.AsDate());
        }

        [Fact]
        public void Type_Value_Test()
        {
            var typeValue = new TypeTypeValue(new TypeDescriptor
            {
                Name = "Строка",
                ID = 1899
            });
            Assert.True(typeValue.DataType == DataType.Type);
            Assert.True(typeValue.AsString() == "Строка");

            Assert.Throws<RuntimeException>(() => typeValue.AsNumber());
            Assert.Throws<RuntimeException>(() => typeValue.AsBoolean());
            Assert.Throws<RuntimeException>(() => typeValue.AsObject());
            Assert.Throws<RuntimeException>(() => typeValue.AsDate());

        }

        [Fact]
        public void Number_To_Number_Comparison()
        {
            var num1 = ValueFactory.Create(1);
            var num2 = ValueFactory.Create(2);
            var num3 = ValueFactory.Create(1);

            Assert.True(num1.CompareTo(num2) < 0);
            Assert.True(num1.CompareTo(num3) == 0);
            Assert.True(num2.CompareTo(num1) > 0);
            Assert.True(num3.CompareTo(num2) < 0);

        }

        [Fact]
        public void Invalid_Comparison_Num_And_String()
        {
            var num1 = ValueFactory.Create(1);
            var num2 = ValueFactory.Create("2");

            Assert.Throws<RuntimeException>(() => num1.CompareTo(num2));
            Assert.Throws<RuntimeException>(() => num2.CompareTo(num1));
        }

        [Fact]
        public void Invalid_Comparison_Undefined()
        {
            var v1 = ValueFactory.Create();
            var v2 = ValueFactory.Create(true);

            try
            {
                v1.CompareTo(v2);
            }
            catch(RuntimeException e)
            {
                var validExc = RuntimeException.ComparisonNotSupportedException();
                Assert.True(e.Message == validExc.Message);
                return;
            }

            throw new Exception("No exception thrown");
        }

        [Fact]
        public void String_To_String_Comparison()
        {
            var str1 = ValueFactory.Create("АБВ");
            var str2 = ValueFactory.Create("ВГД");

            Assert.True(str1.CompareTo(str2) < 0);
            Assert.True(str2.CompareTo(str1) > 0);
            Assert.True(str1.CompareTo(ValueFactory.Create("абв")) != 0);
        }

        [Fact]
        public void Boolean_Comparison()
        {
            var v1 = ValueFactory.Create(true);
            var v2 = ValueFactory.Create(false);
            var num1 = ValueFactory.Create(1);
            var num0 = ValueFactory.Create(0);

            Assert.True(v1.CompareTo(v2) > 0);
            Assert.True(v2.CompareTo(v1) < 0);
            Assert.True(v1.CompareTo(num1) == 0);
            Assert.True(v2.CompareTo(num0) == 0);
            Assert.True(v1.CompareTo(num0) > 0);
            Assert.True(v2.CompareTo(num1) < 0);
            Assert.True(num1.CompareTo(v1) == 0);
            Assert.True(num1.CompareTo(v2) > 0);
        }

        [Theory]
        [InlineData("Null", DataType.GenericValue)]
        [InlineData("Истина", DataType.Boolean)]
        [InlineData("Ложь", DataType.Boolean)]
        [InlineData("True", DataType.Boolean)]
        [InlineData("False", DataType.Boolean)]
        [InlineData("20140105", DataType.Date)]
        [InlineData("20140105010101", DataType.Date)]
        [InlineData("Неопределено", DataType.Undefined)]
        [InlineData("Undefined", DataType.Undefined)]
        public void ValueFactory_Parse(string literal, DataType type)
        {
            var value = ValueFactory.Parse(literal, type);
            Assert.True(value.DataType == type);
        }
    }
}