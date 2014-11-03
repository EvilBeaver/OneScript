using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class UndefinedValue : GenericValue
    {
        public static readonly UndefinedValue Instance = new UndefinedValue();

        private UndefinedValue()
        {

        }

        public override DataType Type
        {
            get
            {
                return BasicTypes.Undefined;
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
