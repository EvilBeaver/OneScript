using System;

namespace ScriptEngine.Machine
{
    class StringConstantValue : IValue
    {
        private readonly string _value;

        public StringConstantValue(string val)
        {
            if (val == null)
                throw new ArgumentNullException();

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
            if(other.DataType == DataType.String)
                return _value.CompareTo(other.AsString());

            throw RuntimeException.ComparisonNotSupportedException();
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
    }
}