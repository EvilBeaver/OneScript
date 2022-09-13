/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Types;

namespace OneScript.Values
{
    public sealed class BslTypeValue : BslPrimitiveValue
    {
        private readonly TypeDescriptor _type;

        public BslTypeValue(TypeDescriptor type)
        {
            _type = type;
        }
        
        public override TypeDescriptor SystemType => BasicTypes.Type;

        public override string ToString()
        {
            return _type.ToString();
        }

        public override bool Equals(BslValue other)
        {
            if(other?.SystemType == BasicTypes.Type)
            {
                var otherVal = other.GetRawValue() as BslTypeValue;
                return otherVal._type == this._type;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is BslTypeValue value)
                return Equals(value);
            return false;
        }

        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }

        public TypeDescriptor TypeValue => _type;
    }
}