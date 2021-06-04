/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace NUnitTests
{
    [TestFixture]
    public class ContextTests : ISystemLogWriter
    {
        private EngineWrapperNUnit host;
        private readonly List<string> _messages = new List<string>();

        [OneTimeSetUp]
        public void Init()
        {
            host = new EngineWrapperNUnit();
            host.StartEngine();
            var solutionRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");
            host.Engine.InitExternalLibraries(Path.Combine(solutionRoot, "oscript-library", "src"), null);
            host.Engine.AttachAssembly(typeof(TestContextClass).Assembly);
        }

        [Test]
        public void TestICallDeprecatedAndHaveWarning()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();
            host.RunTestString(
                @"К = Новый ТестовыйКласс;
				К.УстаревшийМетод();
				К.ObsoleteMethod();
				К.УстаревшийМетод();");

            Assert.AreEqual(1, _messages.Count, "Только ОДНО предупреждение");
            Assert.IsTrue(_messages[0].IndexOf("УстаревшийМетод", StringComparison.InvariantCultureIgnoreCase) >= 0
                          || _messages[0].IndexOf("ObsoleteMethod", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        [Test]
        public void CheckUndefinedIsNull0()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат Неопределено в функцию IValue
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.5;
                ВЗ = К.ТестIValueНеопределено(Арг);
                Если Не ВЗ = Неопределено Тогда
                    ВызватьИсключение ""Test IValue Func(IValue) -> Func(Unknown): return value is not equal Undefined"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull1()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат значения в функцию IValue
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = 7.5;
                ВЗ = 5.7;
                ВЗ = К.ТестIValue(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test IValue Func(IValue) -> Func(IValue): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull2()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат значения Class в функцию Class
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Новый ПNullClass;
                ВЗ = 5.7;
                ВЗ = К.ТестКласс(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test Class Func(Class) -> Func(Class): return value is not equal of argument"";
                КонецЕсли;");
        }
        [Test]
        public void CheckUndefinedIsNull3()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат значения Неопределено в функцию Class
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестClassНеопределено(Арг);
                Если Не ВЗ = Неопределено Тогда
                    ВызватьИсключение ""Test Class Func(Class) -> Func(Unknown): return value is not equal of Unknown"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull4()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат значения в функцию Строка
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = ""привет"";
                ВЗ = 5.7;
                ВЗ = К.ТестString(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test string Func(string) -> Func(string): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull5()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат Неопределено в функцию Строка
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестNullString(Арг);
                Если Не ВЗ = Неопределено Тогда
                    ВызватьИсключение ""Test string Func(string) -> Func(Unknown): return value is not equal of null"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull6()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат double в функцию
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = 3.6;
                ВЗ = 5.7;
                ВЗ = К.ТестDouble(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test double Func(double) -> Func(double): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull7()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат double в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестDouble(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test double Func(double) -> Func(Unknown): must raise exception");
        }

        [Test]
        public void CheckUndefinedIsNull8()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат int в функцию
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = 3;
                ВЗ = 5.7;
                ВЗ = К.ТестInt(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test int Func(int) -> Func(int): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull9()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат Неопределено в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестInt(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test int Func(int) -> Func(Unknown): must raise exception");
        }

        [Test]
        public void CheckUndefinedIsNull10()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            host.RunTestString(
           @"К = Новый ТестNullПреобразования;
                Арг = 3;
                ВЗ = 5.7;
                ВЗ = К.ТестUInt(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test uint Func(uint) -> Func(uint): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull11()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат Неопределено в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестUInt(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test uint Func(uint) -> Func(Unknown): must raise exception");
        }

        [Test]
        public void CheckUndefinedIsNull12()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат long в функцию
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = 3;
                ВЗ = 5.7;
                ВЗ = К.ТестLong(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test long Func(long) -> Func(long): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull13()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат Неопределено в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестLong(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test long Func(long) -> Func(Unknown): must raise exception");
        }

        [Test]
        public void CheckUndefinedIsNull14()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат ulong в функцию
            host.RunTestString(
           @"К = Новый ТестNullПреобразования;
                Арг = 3;
                ВЗ = 5.7;
                ВЗ = К.ТестULong(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test ulong Func(ulong) -> Func(ulong): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull15()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат Неопределено в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестULong(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test ulong Func(ulong) -> Func(Unknown): must raise exception");
        }

        [Test]
        public void CheckUndefinedIsNull16()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат DateTime в функцию
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = ТекущаяДата();
                ВЗ = 5.7;
                ВЗ = К.ТестDateTime(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test DateTime Func(DateTime) -> Func(DateTime): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull17()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат Неопределено в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестDateTime(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test DateTime Func(DateTime) -> Func(Unknown): must raise exception");
        }

        [Test]
        public void CheckUndefinedIsNull18()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат bool в функцию
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Истина;
                ВЗ = 5.7;
                ВЗ = К.ТестBool(Арг);
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test bool Func(bool) -> Func(bool): return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull19()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            bool wasException = false;
            // Передача и возврат Неопределено в функцию
            try
            {
                host.RunTestString(
                @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                ВЗ = 5.7;
                ВЗ = К.ТестBool(Арг);");
            }
            catch (Exception)
            {
                wasException = true;
            }

            if (!wasException)
                Assert.Fail("Test bool Func(bool) -> Func(Unknown): must raise exception");
        }
        //
        // Чтение/запись свойств
        //
        // Передача и возврат ivalue

        [Test]
        public void CheckUndefinedIsNull20()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = 3.5;
                К.ПIValue = Арг; 
                ВЗ = К.КПIValue;
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test IValue Prop <-> IValue: return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull21()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат ivalue
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                К.ПNullIValue = Арг; 
                ВЗ = К.ПNullIValue;
                Если Не ВЗ = Неопределено Тогда
                    ВызватьИсключение ""Test IValue Prop <-> Unknown: return value is not equal of Unknown"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull22()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();


            // Передача и возврат Class
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Новый ТестNullКласс;
                К.ПClass = Арг; 
                ВЗ = К.ПClass;
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test Class Prop <-> Class: return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull23()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат Class
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                К.ПNullClass = Арг; 
                ВЗ = К.ПNullClass;
                Если Не ВЗ = Неопределено Тогда
                    ВызватьИсключение ""Test Class Prop <-> Unknown: return value is not equal of Unknown"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull24()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат string
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = ""hello"";
                К.ПString = Арг; 
                ВЗ = К.ПString;
                Если Не ВЗ = Арг Тогда
                    ВызватьИсключение ""Test string Prop <-> string: return value is not equal of argument"";
                КонецЕсли;");
        }

        [Test]
        public void CheckUndefinedIsNull25()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();

            // Передача и возврат string
            host.RunTestString(
            @"К = Новый ТестNullПреобразования;
                Арг = Неопределено;
                К.ПNullString = Арг; 
                ВЗ = К.ПNullString;
                Если Не ВЗ = Неопределено Тогда
                    ВызватьИсключение ""Test string Prop <-> Unknown: return value is not equal of Unknown"";
                КонецЕсли;");
        }

        [Test]
        public void TestICallGoodAndHaveNoWarning()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();
            host.RunTestString(
                @"К = Новый ТестовыйКласс;
				К.ХорошийМетод();
				К.GoodMethod();");

            Assert.AreEqual(0, _messages.Count, "Нет предупреждений");
        }

        [Test]
        public void TestICallDeprecatedAliasAndHaveWarning()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();
            host.RunTestString(
                @"К = Новый ТестовыйКласс;
				К.ObsoleteAlias();");

            Assert.AreEqual(1, _messages.Count, "Только ОДНО предупреждение");
            Assert.IsTrue(_messages[0].IndexOf("ObsoleteAlias", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        [Test]
        public void TestICallDeprecatedAliasAndHaveException()
        {
            SystemLogger.SetWriter(this);
            _messages.Clear();
            var exceptionThrown = false;

            try
            {
                host.RunTestString(
                    @"К = Новый ТестовыйКласс;
					К.VeryObsoleteAlias();");
            }
            catch (RuntimeException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Безнадёжно устаревший метод должен вызвать исключение");
        }

        public void Write(string text)
        {
            _messages.Add(text);
        }

    }
}