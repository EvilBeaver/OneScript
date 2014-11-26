using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class TypeTypeValue : IValue
    {
        TypeDescriptor _instance;

        public TypeTypeValue(string name)
        {
            _instance = TypeManager.GetTypeByName(name);
        }

        public TypeTypeValue(TypeDescriptor type)
        {
            _instance = type;
        }

        public DataType DataType
        {
            get { return Machine.DataType.Type; }
        }

        public TypeDescriptor SystemType
        {
            get { return TypeDescriptor.FromDataType(DataType.Type); }
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
            return _instance.ToString();
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
            if(other.DataType == this.DataType)
            {
                var otherVal = other.GetRawValue() as TypeTypeValue;
                return otherVal._instance.ID == this._instance.ID;
            }
            else
            {
                return false;
            }
        }

    }
}
