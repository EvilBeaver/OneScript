using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class DateValue : GenericValue
    {
        private DateTime _value;

        public DateValue(DateTime date)
        {
            _value = date;
        }

        public override DataType Type
        {
            get
            {
                return BasicTypes.Date;
            }
        }

        public override string AsString()
        {
            return _value.ToString();
        }

        public override DateTime AsDate()
        {
            return _value;
        }

        protected override int CompareSameType(IValue other)
        {
            var dt = other.AsDate();
            return _value.CompareTo(dt);
        }

        public override bool Equals(IValue other)
        {
            if (other.Type == BasicTypes.Date)
                return _value.Equals(other.AsDate());
            else
                return false;
        }
    }
}
