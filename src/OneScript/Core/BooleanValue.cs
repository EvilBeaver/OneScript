using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class BooleanValue : GenericValue
    {
        public static readonly BooleanValue True = new BooleanValue(true);
        public static readonly BooleanValue False = new BooleanValue(false);
        
        private bool _value = false;

        public BooleanValue(bool valueToSet)
        {
            _value = valueToSet;
        }

        public override DataType Type
        {
            get
            {
                return BasicTypes.Boolean;
            }
        }

        public override string ToString()
        {
            return _value == true ? "Да" : "Нет";
        }

        public override bool AsBoolean()
        {
            return _value;
        }

        public override decimal AsNumber()
        {
            return _value == true ? 1 : 0;
        }

        protected override int CompareSameType(IValue other)
        {
            return _value.CompareTo(other.AsBoolean());
        }

        public override bool Equals(IValue other)
        {
            return _value == other.AsBoolean();
        }
    }
}
