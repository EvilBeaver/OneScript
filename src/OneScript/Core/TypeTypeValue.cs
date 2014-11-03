using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class TypeTypeValue : GenericValue
    {

        public TypeTypeValue(DataType type)
        {
            ReferencedType = type;
        }

        public DataType ReferencedType { get; private set; }

        public override DataType Type
        {
            get { return BasicTypes.Type; }
        }

        public override string AsString()
        {
            return ReferencedType.ToString();
        }

        public override bool Equals(IValue other)
        {
            if (other.Type == this.Type)
            {
                var ttv = (TypeTypeValue)other;
                return ttv.ReferencedType.Equals(this.ReferencedType);
            }
            else
            {
                return false;
            }
        }

        protected override int CompareSameType(IValue other)
        {
            var ttv = other as TypeTypeValue;
            return this.ReferencedType.CompareTo(ttv.ReferencedType);
        }
    }
}
