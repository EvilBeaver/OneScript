using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class NumericValue : GenericValue
    {
        public static readonly NumericValue Zero = new NumericValue(0);
        public static readonly NumericValue One = new NumericValue(1);
        public static readonly NumericValue MinusOne = new NumericValue(-1);

        private decimal _value;

        public NumericValue(decimal value)
        {
            _value = value;
        }

        public override DataType Type
        {
            get
            {
                return BasicTypes.Number;
            }
        }

        public override string ToString()
        {
            return _value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public override bool AsBoolean()
        {
            return _value != 0;
        }

        public override decimal AsNumber()
        {
            return _value;
        }

        public override int CompareTo(IValue other)
        {
            if (other.Type == BasicTypes.Number || other.Type == BasicTypes.Boolean)
            {
                return this.AsNumber().CompareTo(other.AsNumber());
            }
            else
                throw TypeConversionException.ComparisonIsNotSupportedException();
        }
        
        public override bool Equals(IValue other)
        {
            return _value.Equals(other.AsNumber());
        }

    }
}
