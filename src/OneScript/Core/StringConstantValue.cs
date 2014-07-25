using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneScript.Core
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

        public DataType Type
        {
            get { return BasicTypes.String; }
        }

        public double AsNumber()
        {
            return ValueFactory.Parse(_value, BasicTypes.Number).AsNumber();
        }

        public DateTime AsDate()
        {
            return ValueFactory.Parse(_value, BasicTypes.Date).AsDate();
        }

        public bool AsBoolean()
        {
            return ValueFactory.Parse(_value, BasicTypes.Boolean).AsBoolean();
        }

        public string AsString()
        {
            return _value;
        }

        public IRuntimeContextInstance AsObject()
        {
            throw TypeConversionException.ConvertToObjectException();
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
            if (other.Type == this.Type)
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
