/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using NUnit.Framework;

namespace NUnitTests
{
    [ContextClass("HTTPСервисЗапрос", "HTTPServiceRequest")]
    class TestNullConversion : AutoContext<TestNullConversion>
    {
        IValue _pIValue;
        string _pString;
        int _pInt;
        uint _pUint;
        long _pLong;
        ulong _pUlong;
        double _pDouble;
        DateTime _pDateTime;
        bool _pBool;
        TestNullClass _pClass;


        public TestNullConversion()
        {

        }

        [ScriptConstructor(Name = "Без параметров")]
        public static IRuntimeContextInstance Constructor()
        {
            return new TestNullConversion();
        }

        [ContextMethod("ТестIValue", "IValueTest")]
        public IValue TestIValue(IValue arg)
        {
            if (arg.GetType() != typeof(IValue))
                Assert.Fail("Тест IValue: Передаваемый параметр имеет тип, отличный от IValue.");

            return arg;
        }

        [ContextMethod("ТестIValueНеопределено", "IValueNullTest")]
        public IValue TestIValueNull(IValue arg)
        {
            if (arg.GetType() != typeof(IValue))
                Assert.Fail("Тест IValue: Передаваемый параметр имеет тип, отличный от IValue.");

            if (arg != ValueFactory.Create())
                Assert.Fail("Тест IValue (Неопределено): Передаваемый параметр имеет значение, отличное от ValueFactory.Create().");

            return arg;
        }

        [ContextMethod("ТестКласс", "ClassTest")]
        public TestNullClass TestClass(TestNullClass arg)
        {
            if (arg.GetType() != typeof(TestNullClass))
                Assert.Fail("Тест IValue: Передаваемый параметр имеет тип, отличный от TestNullClass.");

            return arg;
        }

        [ContextMethod("ТестClassНеопределено", "ClassNullTest")]
        public TestNullClass TestClassNull(TestNullClass arg)
        {
            if (arg.GetType() != typeof(TestNullClass))
                Assert.Fail("Тест IValue: Передаваемый параметр имеет тип, отличный от TestNullClass.");

            if (arg != null)
                Assert.Fail("Тест Класс (Неопределено): Передаваемый параметр имеет значение, отличное от null.");

            return arg;
        }



        [ContextMethod("ТестString", "StringTest")]
        public string TestString(string arg)
        {
            if (arg.GetType() != typeof(System.String))
                Assert.Fail("Тест String: Передаваемый параметр имеет тип, отличный от System.String.");

            return arg;
        }

        [ContextMethod("ТестNullString", "StringNullTest")]
        public string TestStringNull(string arg)
        {
            if (arg != null)
                Assert.Fail("Тест String: Передаваемый параметр имеет тип, отличный от System.String.");

            return arg;
        }

        [ContextMethod("ТестInt", "IntTest")]
        public int TestInt(int arg)
        {
            return arg;
        }

        [ContextMethod("ТестUInt", "UIntTest")]
        public uint TestUInt(uint arg)
        {
            return arg;
        }

        [ContextMethod("ТестLong", "LongTest")]
        public long TestLong(long arg)
        {
            return arg;
        }

        [ContextMethod("ТестULong", "ULongTest")]
        public ulong TestULong(ulong arg)
        {
            return arg;
        }

        [ContextMethod("ТестDouble", "DoubleTest")]
        public double TestDouble(double arg)
        {
            return arg;
        }

        [ContextMethod("ТестDateTime", "DateTimeTest")]
        public DateTime TestDateTime(DateTime arg)
        {
            return arg;
        }

        [ContextMethod("ТестBool", "BoolTest")]
        public bool TestBool(bool arg)
        {
            return arg;
        }

        [ContextProperty("ПInt", "PInt")]
        public int PInt
        {
            get
            {
                return _pInt;
            }
            set
            {
                _pInt = value;
            }
        }

        [ContextProperty("ПUint", "PUint")]
        public uint PUint
        {
            get
            {
                return _pUint;
            }
            set
            {
                _pUint = value;
            }
        }

        [ContextProperty("ПLong", "PLong")]
        public long PLong
        {
            get
            {
                return _pLong;
            }
            set
            {
                _pLong = value;
            }
        }

        [ContextProperty("ПUlong", "PUlong")]
        public ulong PUlong
        {
            get
            {
                return _pUlong;
            }
            set
            {
                _pUlong = value;
            }
        }

        [ContextProperty("ПDouble", "PDouble")]
        public double PDouble
        {
            get
            {
                return _pDouble;
            }
            set
            {
                _pDouble = value;
            }
        }

        [ContextProperty("ПDateTime", "PDateTime")]
        public DateTime PDateTime
        {
            get
            {
                return _pDateTime;
            }
            set
            {
                _pDateTime = value;
            }
        }

        [ContextProperty("ПBool", "PBool")]
        public bool PBool
        {
            get
            {
                return _pBool;
            }
            set
            {
                _pBool = value;
            }
        }

        [ContextProperty("ПString", "PString")]
        public string PString
        {
            get
            {
                return _pString;
            }
            set
            {
                _pString = value;
            }
        }

        [ContextProperty("ПNullString", "PNullString")]
        public string PNullString
        {
            get
            {
                return _pString;
            }
            set
            {
                if (value != null)
                    Assert.Fail("Тест ПNullString: Передаваемый параметр имеет значение, отличное от null.");

                _pString = value;
            }
        }

        [ContextProperty("ПIValue", "PIValue")]
        public IValue PIValue
        {
            get
            {
                return _pIValue;
            }
            set
            {
                if (value.GetType() != typeof(IValue))
                    Assert.Fail("Тест ПString: Передаваемый параметр имеет тип, отличный от IValue.");

                _pIValue = value;
            }
        }

        [ContextProperty("ПNullIValue", "PNullIValue")]
        public IValue PNullIValue
        {
            get
            {
                return _pIValue;
            }
            set
            {
                if (value.GetType() != typeof(IValue))
                    Assert.Fail("Тест ПNullValue: Передаваемый параметр имеет тип, отличный от IValue.");
                if (value != ValueFactory.Create())
                    Assert.Fail("Тест ПNullIValue: Передаваемый параметр имеет значение, отличное от ValueFactory.Create().");

                _pIValue = value;
            }
        }



        [ContextProperty("ПClass", "PClass")]
        public TestNullClass PClass
        {
            get
            {
                return _pClass;
            }
            set
            {
                if (value.GetType() != typeof(TestNullClass))
                    Assert.Fail("Тест ПClass: Передаваемый параметр имеет тип, отличный от TestNullClass.");

                _pClass = value;
            }
        }

        [ContextProperty("ПNullClass", "PNullClass")]
        public TestNullClass PNullClass
        {
            get
            {
                return _pClass;
            }
            set
            {
                if (value != null)
                    Assert.Fail("Тест ПNullClass: Передаваемый параметр имеет значение, отличное от null.");
                _pIValue = value;
            }
        }

    }

    [ContextClass("ТестNullКласс", "TestNullClass")]
    class TestNullClass : AutoContext<TestNullClass>
    {
        public TestNullClass()
        {

        }

        [ScriptConstructor(Name = "Без параметров")]
        public static IRuntimeContextInstance Constructor()
        {
            return new TestNullClass();
        }

    }

}
