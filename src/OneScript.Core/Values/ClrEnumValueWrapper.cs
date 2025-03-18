/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Types;

namespace OneScript.Values
{
    public class ClrEnumValueWrapper<T> : EnumerationValue, IObjectWrapper where T :struct
    {
        private readonly T _realValue;

        public ClrEnumValueWrapper(TypeDescriptor systemType, T realValue, string name, string alias)
        : base (systemType, name, alias)
        { 
            _realValue = realValue;
        }

        public object UnderlyingObject => _realValue;

        public T UnderlyingValue  => _realValue;

        public override bool Equals(BslValue other)
        {
            if (!(other?.GetRawValue() is ClrEnumValueWrapper<T> otherWrapper))
                return false;

            return UnderlyingValue.Equals(otherWrapper.UnderlyingValue);
        }

        public bool Equals(ClrEnumValueWrapper<T> otherWrapper)
        {
            return UnderlyingValue.Equals(otherWrapper.UnderlyingValue);
        }
    }
}
