using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class NullValueImpl : IValue
    {

        public DataType DataType
        {
            get { return Machine.DataType.GenericValue; }
        }

        public TypeDescriptor SystemType
        {
            get { return TypeManager.GetTypeByFrameworkType(typeof(NullValueImpl)); }
        }

        public decimal AsNumber()
        {
            throw RuntimeException.ConvertToNumberException();
        }

        public DateTime AsDate()
        {
            throw RuntimeException.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            throw RuntimeException.ConvertToBooleanException();
        }

        public string AsString()
        {
            return "";
        }

        public IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public int CompareTo(IValue other)
        {
            throw RuntimeException.ComparisonNotSupportedException();
        }

        public bool Equals(IValue other)
        {
            return other.GetRawValue() == Instance;
        }

        public static readonly NullValueImpl Instance;

        static NullValueImpl()
        {
            Instance = new NullValueImpl();
        }

    }
}
