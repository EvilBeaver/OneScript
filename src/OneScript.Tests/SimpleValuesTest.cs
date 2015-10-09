using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class SimpleValuesTest
    {
        [TestMethod]
        public void ValueFactory_Creates_CorrectTypes()
        {
            IValue value;
            value = ValueFactory.Create();
            Assert.IsTrue(value.Type == BasicTypes.Undefined, "Undefined test failed");

            value = ValueFactory.Create("string value");
            Assert.IsTrue(value.Type == BasicTypes.String, "String test failed");
            Assert.IsTrue(value.AsString() == "string value", "String value test failed");

            value = ValueFactory.Create(true);
            Assert.IsTrue(value.Type == BasicTypes.Boolean, "Boolean 1 test failed");
            Assert.IsTrue(value.AsBoolean() == true, "True test failed");
            
            value = ValueFactory.Create(false);
            Assert.IsTrue(value.Type == BasicTypes.Boolean, "Boolean 2 test failed");
            Assert.IsTrue(value.AsBoolean() == false, "False test failed");

            value = ValueFactory.Create(1.56m);
            Assert.IsTrue(value.Type == BasicTypes.Number, "Number test failed");
            Assert.IsTrue(value.AsNumber() == 1.56m, "Number value test failed");

            var today = DateTime.Today;
            value = ValueFactory.Create(today);
            Assert.IsTrue(value.Type == BasicTypes.Date, "Date test failed");
            Assert.IsTrue(value.AsDate() == today, "Date value test failed");

            value = ValueFactory.Create(PredefinedValueType.Null);
            Assert.IsTrue(value.Type == BasicTypes.Null, "Null test failed");
        }

        [TestMethod]
        public void Boolean_Value_Test()
        {
            var first = ValueFactory.Create(true);
            var second = ValueFactory.Create(true);
            Assert.AreSame(first, second);
            Assert.IsTrue(first.Equals(second));

            second = ValueFactory.Create(false);
            Assert.AreSame(second, ValueFactory.Create(false));

            Assert.IsTrue(first.AsBoolean());
            Assert.IsTrue(first.AsNumber() == 1);
            Assert.IsTrue(first.AsString() == "Да");

            Assert.IsFalse(second.AsBoolean());
            Assert.IsTrue(second.AsNumber() == 0);
            Assert.IsTrue(second.AsString() == "Нет");

            Assert.IsTrue(first.CompareTo(second) > 0);

            Assert.IsTrue(ExceptionThrown(()=>first.AsDate(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => first.AsObject(), typeof(TypeConversionException)));

        }

        [TestMethod]
        public void Number_Value_Test()
        {
            var first = ValueFactory.Create(1);
            var second = ValueFactory.Create(1);
            Assert.IsTrue(first.Equals(second));
            Assert.AreSame(first, second);
            Assert.IsTrue(first.AsNumber() == 1);
            Assert.IsTrue(first.CompareTo(second) == 0);
            Assert.IsTrue(first.AsBoolean() == true);

            first = ValueFactory.Create(0);
            second = ValueFactory.Create(1.56);
            Assert.IsFalse(first.Equals(second));
            Assert.AreNotSame(first, second);
            Assert.IsTrue(first.AsNumber() == 0);
            Assert.IsTrue(first.CompareTo(second) < 0);
            Assert.IsTrue(first.AsBoolean() == false);
            Assert.IsTrue(second.AsBoolean() == true);

            Assert.IsTrue(second.AsString() == "1.56");

            Assert.IsTrue(ExceptionThrown(() => first.AsDate(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => first.AsObject(), typeof(TypeConversionException)));
        }

        [TestMethod]
        public void String_Value_test()
        {
            var trueString = ValueFactory.Create("ИстИНа");
            Assert.IsTrue(trueString.Type == BasicTypes.String);
            Assert.IsTrue(trueString.AsBoolean());
            Assert.IsTrue(trueString.AsString() == "ИстИНа");

            var falseString = ValueFactory.Create("лОжЬ");
            Assert.IsTrue(falseString.Type == BasicTypes.String);
            Assert.IsFalse(falseString.AsBoolean());
            Assert.IsTrue(falseString.AsString() == "лОжЬ");

            var dateString = ValueFactory.Create("20140101");
            DateTime jan_01_14 = new DateTime(2014,01,01);
            Assert.IsTrue(dateString.AsDate() == jan_01_14);

            var numString = ValueFactory.Create("012.12");
            Assert.IsTrue(numString.AsNumber() == 12.12m);

            Assert.IsTrue(ExceptionThrown(() => dateString.AsObject(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => trueString.AsNumber(), typeof(TypeConversionException)));
        }

        [TestMethod]
        public void Undefined_Value_Test()
        {
            var value = ValueFactory.Create();
            Assert.IsTrue(value.Type == BasicTypes.Undefined);
            Assert.IsTrue(value.AsString() == "");

            Assert.IsTrue(ExceptionThrown(() => value.AsNumber(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => value.AsBoolean(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => value.AsObject(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => value.AsDate(), typeof(TypeConversionException)));
        }

        [TestMethod]
        public void Null_Value_Test()
        {
            var value = ValueFactory.Create(PredefinedValueType.Null);
            Assert.IsTrue(value.Type == BasicTypes.Null);
            Assert.IsTrue(value.AsString() == "");

            Assert.IsTrue(ExceptionThrown(() => value.AsNumber(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => value.AsBoolean(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => value.AsObject(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => value.AsDate(), typeof(TypeConversionException)));
        }

        [TestMethod]
        public void Type_Value_Test()
        {
            var typeValue = ValueFactory.Create(BasicTypes.String);
            Assert.IsTrue(typeValue.Type == BasicTypes.Type);
            Assert.IsTrue(typeValue.AsString() == "Строка");
            Assert.IsTrue(typeValue.Equals(ValueFactory.Create(BasicTypes.String)));

            Assert.IsTrue(ExceptionThrown(() => typeValue.AsNumber(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => typeValue.AsBoolean(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => typeValue.AsObject(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => typeValue.AsDate(), typeof(TypeConversionException)));

        }

        [TestMethod]
        public void Number_To_Number_Comparison()
        {
            var num1 = ValueFactory.Create(1);
            var num2 = ValueFactory.Create(2);
            var num3 = ValueFactory.Create(1);

            Assert.IsTrue(num1.CompareTo(num2) < 0);
            Assert.IsTrue(num1.CompareTo(num3) == 0);
            Assert.IsTrue(num2.CompareTo(num1) > 0);
            Assert.IsTrue(num3.CompareTo(num2) < 0);

        }

        [TestMethod]
        public void Invalid_Comparison_Num_And_String()
        {
            var num1 = ValueFactory.Create(1);
            var num2 = ValueFactory.Create("2");

            Assert.IsTrue(TestHelpers.ExceptionThrown(() => num1.CompareTo(num2), typeof(TypeConversionException)));
            Assert.IsTrue(TestHelpers.ExceptionThrown(() => num2.CompareTo(num1), typeof(TypeConversionException)));
        }

        [TestMethod]
        public void Invalid_Comparison_Undefined()
        {
            var v1 = ValueFactory.Create();
            var v2 = ValueFactory.Create(true);

            try
            {
                v1.CompareTo(v2);
            }
            catch(TypeConversionException e)
            {
                var validExc = TypeConversionException.ComparisonIsNotSupportedException();
                Assert.IsTrue(e.Message == validExc.Message);
                return;
            }

            Assert.Fail("No exception thrown");
        }

        [TestMethod]
        public void String_To_String_Comparison()
        {
            var str1 = ValueFactory.Create("АБВ");
            var str2 = ValueFactory.Create("ВГД");

            Assert.IsTrue(str1.CompareTo(str2) < 0);
            Assert.IsTrue(str2.CompareTo(str1) > 0);
            Assert.IsTrue(str1.CompareTo(ValueFactory.Create("абв")) != 0);
        }

        [TestMethod]
        public void Boolean_Comparison()
        {
            var v1 = ValueFactory.Create(true);
            var v2 = ValueFactory.Create(false);
            var num1 = ValueFactory.Create(1);
            var num0 = ValueFactory.Create(0);

            Assert.IsTrue(v1.CompareTo(v2) > 0);
            Assert.IsTrue(v2.CompareTo(v1) < 0);
            Assert.IsTrue(v1.CompareTo(num1) == 0);
            Assert.IsTrue(v2.CompareTo(num0) == 0);
            Assert.IsTrue(v1.CompareTo(num0) > 0);
            Assert.IsTrue(v2.CompareTo(num1) < 0);
            Assert.IsTrue(num1.CompareTo(v1) == 0);
            Assert.IsTrue(num1.CompareTo(v2) > 0);

        }

        private bool ExceptionThrown(Action action, Type exceptionType)
        {
            return TestHelpers.ExceptionThrown(action, exceptionType);
        }

        [TestMethod]
        public void Value_Factory_Parses_Undefined()
        {
            var value = ValueFactory.Parse("Неопределено", BasicTypes.Undefined);
            Assert.IsTrue(value.Type == BasicTypes.Undefined);
        }

        [TestMethod]
        public void Value_Factory_Parses_Null()
        {
            var value = ValueFactory.Parse("Null", BasicTypes.Null);
            Assert.IsTrue(value.Type == BasicTypes.Null);
        }

        [TestMethod]
        public void Value_Factory_Parses_Boolean()
        {
            IValue value;
            value = ValueFactory.Parse("Истина", BasicTypes.Boolean);
            Assert.IsTrue(value.Type == BasicTypes.Boolean);

            value = ValueFactory.Parse("Ложь", BasicTypes.Boolean);
            Assert.IsTrue(value.Type == BasicTypes.Boolean);

            value = ValueFactory.Parse("True", BasicTypes.Boolean);
            Assert.IsTrue(value.Type == BasicTypes.Boolean);

            value = ValueFactory.Parse("False", BasicTypes.Boolean);
            Assert.IsTrue(value.Type == BasicTypes.Boolean);
        }

        [TestMethod]
        public void Value_Factory_Parses_Date()
        {
            var value = ValueFactory.Parse("20140105", BasicTypes.Date);
            Assert.IsTrue(value.Type == BasicTypes.Date);
            value = ValueFactory.Parse("20140105010101", BasicTypes.Date);
            Assert.IsTrue(value.Type == BasicTypes.Date);
        }
    }
}
