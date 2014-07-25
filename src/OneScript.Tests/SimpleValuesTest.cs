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

            value = ValueFactory.Create(1.56);
            Assert.IsTrue(value.Type == BasicTypes.Number, "Number test failed");
            Assert.IsTrue(value.AsNumber() == 1.56, "Number value test failed");

            var today = DateTime.Today;
            value = ValueFactory.Create(today);
            Assert.IsTrue(value.Type == BasicTypes.Date, "Date test failed");
            Assert.IsTrue(value.AsDate() == today, "Date value test failed");
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
            Assert.AreNotSame(first, second);
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

            Assert.IsTrue(ExceptionThrown(() => dateString.AsObject(), typeof(TypeConversionException)));
            Assert.IsTrue(ExceptionThrown(() => trueString.AsNumber(), typeof(TypeConversionException)));
        }

        [TestMethod]
        public void Undefined_Value_Test()
        {
            var value = ValueFactory.Create();
            Assert.IsTrue(value.Type == BasicTypes.Undefined);
            Assert.IsTrue(value.AsString() == "Неопределено");

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

        private bool ExceptionThrown(Action action, Type exceptionType)
        {
            try
            {
                action();
                return false;
            }
            catch(Exception e)
            {
                if(e.GetType() == exceptionType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
