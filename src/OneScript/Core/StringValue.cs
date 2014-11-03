using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class StringValue : GenericValue
    {
        private string _value;

        public StringValue(string val)
        {
            Trace.Assert(val != null);
            _value = val;
        }

        public override string ToString()
        {
            return _value;
        }

        public override DataType Type
        {
            get { return BasicTypes.String; }
        }

        public override decimal AsNumber()
        {
            return ValueFactory.Parse(_value, BasicTypes.Number).AsNumber();
        }

        public override DateTime AsDate()
        {
            return ValueFactory.Parse(_value, BasicTypes.Date).AsDate();
        }

        public override bool AsBoolean()
        {
            return ValueFactory.Parse(_value, BasicTypes.Boolean).AsBoolean();
        }

        public override string AsString()
        {
            return _value;
        }

        public override bool Equals(IValue other)
        {
            if (other.Type == BasicTypes.String)
                return String.Equals(_value, other.AsString());
            else
                return false;
        }

        protected override int CompareSameType(IValue other)
        {
            var otherStr = other.AsString();
            return _value.CompareTo(otherStr);
        }

    }
}
