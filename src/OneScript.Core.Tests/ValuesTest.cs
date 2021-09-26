/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Moq;
using OneScript.Commons;
using OneScript.StandardLibrary;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ValuesTest
    {
        [Fact]
        public void BooleanEquality()
        {
            Assert.True(BslBooleanValue.True.AsBoolean());
            Assert.False(BslBooleanValue.False.AsBoolean());

            Assert.Same(BslBooleanValue.True, ValueFactory.Create(true));
            Assert.Same(BslBooleanValue.False, ValueFactory.Create(false));
            Assert.NotEqual(BslBooleanValue.False, BslBooleanValue.True);
            Assert.Equal(0, BslBooleanValue.False.AsNumber());
            Assert.Equal(1, BslBooleanValue.True.AsNumber());

            Assert.True(BslBooleanValue.True.CompareTo(BslBooleanValue.False) > 0);

            Assert.ThrowsAny<TypeConversionException>(() => BslBooleanValue.True.AsDate());
            Assert.ThrowsAny<TypeConversionException>(() => BslBooleanValue.True.AsObject());
        }

        [Theory]
        [InlineData("ru", "Да", "Нет")]
        [InlineData("en", "True", "False")]
        [InlineData("jp", "True", "False")]
        public void BooleanStringLocales(string locale, string trueString, string falseString)
        {
            Locale.SystemLanguageISOName = locale;
            Assert.Equal(trueString, BslBooleanValue.True.AsString());
            Assert.Equal(falseString, BslBooleanValue.False.AsString());
        }

        [Fact]
        public void NumbersEquality()
        {
            var num1 = ValueFactory.Create(12.5M);
            var num2 = ValueFactory.Create(12.5M);
            var num3 = ValueFactory.Create(7);
            var num4 = ValueFactory.Create(0);

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
            Assert.ThrowsAny<TypeConversionException>(() => num1.AsDate());
            Assert.ThrowsAny<TypeConversionException>(() => num1.AsObject());
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
            Assert.True(trueString.SystemType == BasicTypes.String);
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

            Assert.ThrowsAny<TypeConversionException>(() => dateString.AsObject());
            Assert.ThrowsAny<TypeConversionException>(() => trueString.AsNumber());
        }
        
        [Fact]
        public void Undefined_Value_Test()
        {
            var value = ValueFactory.Create();
            Assert.True(value.SystemType == BasicTypes.Undefined);
            Assert.True(value.AsString() == "");

            Assert.ThrowsAny<TypeConversionException>(() => value.AsNumber());
            Assert.ThrowsAny<TypeConversionException>(() => value.AsBoolean());
            Assert.ThrowsAny<TypeConversionException>(() => value.AsObject());
            Assert.ThrowsAny<TypeConversionException>(() => value.AsDate());
        }

        [Fact]
        public void Null_Value_Test()
        {
            var value = ValueFactory.CreateNullValue();
            Assert.True(value is BslNullValue);
            Assert.True(ReferenceEquals(value, BslNullValue.Instance));
            Assert.True(value.AsString() == "");

            Assert.ThrowsAny<TypeConversionException>(() => value.AsNumber());
            Assert.ThrowsAny<TypeConversionException>(() => value.AsBoolean());
            Assert.ThrowsAny<TypeConversionException>(() => value.AsObject());
            Assert.ThrowsAny<TypeConversionException>(() => value.AsDate());
        }

        [Fact]
        public void Type_Value_Test()
        {
            var typeValue = new BslTypeValue(BasicTypes.String);
            Assert.True(typeValue.SystemType == BasicTypes.Type);
            Assert.Equal("Строка", typeValue.AsString());

            Assert.ThrowsAny<TypeConversionException>(() => typeValue.AsNumber());
            Assert.ThrowsAny<TypeConversionException>(() => typeValue.AsBoolean());
            Assert.ThrowsAny<TypeConversionException>(() => typeValue.AsObject());
            Assert.ThrowsAny<TypeConversionException>(() => typeValue.AsDate());

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

            Assert.ThrowsAny<RuntimeException>(() => num1.CompareTo(num2));
            Assert.ThrowsAny<RuntimeException>(() => num2.CompareTo(num1));
        }

        [Fact]
        public void Invalid_Comparison_Undefined()
        {
            var v1 = ValueFactory.Create();
            var v2 = ValueFactory.Create(true);

            //TODO: типизировать исключение ComparisonNotSupportedException
            Assert.ThrowsAny<RuntimeException>(() => v1.CompareTo(v2));
        }

        [Fact]
        public void String_To_String_Comparison()
        {
            var str1 = ValueFactory.Create("АБВ");
            var str2 = ValueFactory.Create("ВГД");

            Assert.True(str1.CompareTo(str2) < 0);
            Assert.True(str2.CompareTo(str1) > 0);
            Assert.True(str1.CompareTo(ValueFactory.Create("абв")) == 0);
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
        [InlineData("Null", DataType.GenericValue, typeof(BslNullValue))]
        [InlineData("Истина", DataType.Boolean, typeof(BslBooleanValue))]
        [InlineData("Ложь", DataType.Boolean, typeof(BslBooleanValue))]
        [InlineData("True", DataType.Boolean, typeof(BslBooleanValue))]
        [InlineData("False", DataType.Boolean, typeof(BslBooleanValue))]
        [InlineData("20140105", DataType.Date, typeof(BslDateValue))]
        [InlineData("20140105010101", DataType.Date, typeof(BslDateValue))]
        [InlineData("Неопределено", DataType.Undefined, typeof(BslUndefinedValue))]
        [InlineData("Undefined", DataType.Undefined, typeof(BslUndefinedValue))]
        public void ValueFactory_Parse(string literal, DataType type, Type implementation)
        {
            var value = ValueFactory.Parse(literal, type);
            Assert.True(value.SystemType.ImplementingClass == implementation);
        }

        [Fact]
        public void TypeEqualityOnCreationTest()
        {
            var autoGuid = new GuidWrapper();
            var manualGuid = new GuidWrapper("9F3457C0-7D2A-4DCD-B9F9-3D9228986A6A");

            var typeManager = new DefaultTypeManager(); 
            var discoverer = new ContextDiscoverer(typeManager, Mock.Of<IGlobalsManager>(), default);
            
            discoverer.DiscoverClasses(typeof(GuidWrapper).Assembly, x => x == typeof(GuidWrapper));

            Assert.True(typeManager.IsKnownType(typeof(GuidWrapper)));
            var typeFromManager = typeManager.GetTypeByFrameworkType(typeof(GuidWrapper));
            
            Assert.Equal(autoGuid.SystemType, typeFromManager);
            Assert.Equal(manualGuid.SystemType, typeFromManager);
            Assert.Equal(manualGuid.SystemType, autoGuid.SystemType);
            
        }

        public static IEnumerable<object[]> FilledValues => new []
        {
            new object[] { ValueFactory.Create(1) },
            new object[] { ValueFactory.Create("hello") },
            new object[] { ValueFactory.Create(DateTime.Now) },
            new object[] { ValueFactory.Create(true) },
            new object[] { ValueFactory.Create(false) },
            new object[] { new GuidWrapper() }
        };
        
        public static IEnumerable<object[]> EmptyValues => new []
        {
            new object[] { ValueFactory.Create(0) },
            new object[] { ValueFactory.Create()},
            new object[] { ValueFactory.Create(DateTime.MinValue)},
            new object[] { ValueFactory.CreateNullValue()},
            new object[] { ValueFactory.Create("")},
            new object[] { ValueFactory.Create("    ")},
            new object[] { new GuidWrapper(Guid.Empty.ToString())}
        };
        
        [Theory]
        [MemberData(nameof(FilledValues))]
        public void ValueFilledTest(IValue value)
        {
            var globCtx = new StandardGlobalContext();
            Assert.True(globCtx.ValueIsFilled(value));
        }
        
        [Theory]
        [MemberData(nameof(EmptyValues))]
        public void ValueEmptyTest(IValue value)
        {
            var globCtx = new StandardGlobalContext();
            Assert.False(globCtx.ValueIsFilled(value));
        }
    }
}