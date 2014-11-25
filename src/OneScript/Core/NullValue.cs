using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class NullValue : GenericValue
    {
        public static readonly NullValue Instance = new NullValue();

        private NullValue()
        {

        }

        public override DataType Type
        {
            get
            {
                return BasicTypes.Null;
            }
        }

        public override string ToString()
        {
            return "";
        }

        public override bool Equals(IValue other)
        {
            return Object.ReferenceEquals(this, Instance);
        }

    }
}
