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

        public override bool AsBoolean()
        {
            return _value != null;
        }

        public override decimal AsNumber()
        {
            return _value;
        }

        protected override int CompareSameType(IValue other)
        {
            return _value.CompareTo(other.AsNumber());
        }

        public override bool Equals(IValue other)
        {
            return _value.Equals(other.AsNumber());
        }

    }
}
