/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit.Sdk;

namespace OneScript.Core.Tests
{
    [ContextClass("ТестNullПреобразования", "TestNullConversion")]
    class NullConversionTestContext : AutoContext<NullConversionTestContext>
    {
        IValue _pIValue;
        string _pString;
        TestNullClass _pClass;


        public NullConversionTestContext()
        {

        }

        [ScriptConstructor(Name = "Без параметров")]
        public static NullConversionTestContext Constructor()
        {
            return new NullConversionTestContext();
        }

        [ContextMethod("ТестIValue", "IValueTest")]
        public IValue TestIValue(IValue arg)
        {
            if (arg == default)
                throw new XunitException("Test IValue Func(IValue) -> Func(IValue): argument is undefined");

            return arg;
        }

        [ContextMethod("ТестIValueНеопределено", "IValueNullTest")]
        public IValue TestIValueNull(IValue arg)
        {
            if (arg != ValueFactory.Create())
                throw new XunitException("Test IValue Func(IValue) -> Func(Unknown): argument value is different from null.");

            return arg;
        }

        [ContextMethod("ТестКласс", "ClassTest")]
        public TestNullClass TestClass(TestNullClass arg)
        {
            if (arg.GetType() != typeof(TestNullClass))
                throw new XunitException("Test Class Func(Class) -> Func(Class): argument type is different from Class.");

            return arg;
        }

        [ContextMethod("ТестClassНеопределено", "ClassNullTest")]
        public TestNullClass TestClassNull(TestNullClass arg)
        {
             if (arg != null)
                 throw new XunitException("Test Class Func(Class) -> Func(Unknown): argument value is different from null.");

            return arg;
        }



        [ContextMethod("ТестString", "StringTest")]
        public string TestString(string arg)
        {
            if (arg.GetType() != typeof(System.String))
                throw new XunitException("Test string Func(string) -> Func(string): argument type is different from string.");

            return arg;
        }

        [ContextMethod("ТестNullString", "StringNullTest")]
        public string TestStringNull(string arg)
        {
            if (arg != null)
                throw new XunitException("Test string Func(string) -> Func(Unknown): argument value is different from null.");

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
            get;set;
        }

        [ContextProperty("ПUint", "PUint")]
        public uint PUint
        {
            get;set;
        }

        [ContextProperty("ПLong", "PLong")]
        public long PLong
        {
            get;set;
         }

        [ContextProperty("ПUlong", "PUlong")]
        public ulong PUlong
        {
            get;set;
        }

        [ContextProperty("ПDouble", "PDouble")]
        public double PDouble
        {
            get;set;
        }

        [ContextProperty("ПDateTime", "PDateTime")]
        public DateTime PDateTime
        {
            get;set;
        }

        [ContextProperty("ПBool", "PBool")]
        public bool PBool
        {
            get;set;
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
                if (value.GetType() != typeof(System.String))
                    throw new XunitException("Test string Property = string: value type is different from string.");

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
                    throw new XunitException("Test string Property = Unknown: value value is different from null.");

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
                    throw new XunitException("Test IValue Property = IValue: value type is different from IValue.");

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
                 if (value != ValueFactory.Create())
                     throw new XunitException("Test IValue Property = Unknown: value value is different from Unknown.");

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
                    throw new XunitException("Test TestNullClass Property = TestNullClass: value type is different from TestNullClass.");

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
                    throw new XunitException("Test TestNullClass Property = Unknown: value value is different from null.");
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
        public static TestNullClass Constructor()
        {
            return new TestNullClass();
        }

    }

}
