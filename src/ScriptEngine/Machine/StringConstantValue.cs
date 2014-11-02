using System;
using System.Diagnostics;

namespace ScriptEngine.Machine
{
    class StringConstantValue : IValue
    {
        private string _value;

        public StringConstantValue(string val)
        {
            Trace.Assert(val != null);
            _value = val;
        }

        public override string ToString()
        {
            return _value;
        }

        #region IValue Members

        public DataType DataType
        {
            get { return DataType.String; }
        }

        public TypeDescriptor SystemType
        {
            get
            {
                return TypeDescriptor.FromDataType(DataType.String);
            }
        }

        public decimal AsNumber()
        {
            return ValueFactory.Parse(_value, DataType.Number).AsNumber();
        }

        public DateTime AsDate()
        {
            return ValueFactory.Parse(_value, DataType.Date).AsDate();
        }

        public bool AsBoolean()
        {
            return ValueFactory.Parse(_value, DataType.Boolean).AsBoolean();
        }

        public string AsString()
        {
            return _value;
        }

        public TypeDescriptor AsType()
        {
            throw new NotImplementedException();
        }

        public IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
        {
            return _value.CompareTo(other.AsString());
        }

        #endregion

        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            if (other.DataType == this.DataType)
            {
                var scv = other.AsString();
                return scv == this._value;
            }
            else
            {
                return false;
            }
        }

        #endregion
        
        #region IRefCountable Members

        public int AddRef()
        {
            return 1;
        }

        public int Release()
        {
            return 1;
        } 
        #endregion
    }
}