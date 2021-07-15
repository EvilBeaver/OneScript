/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;

namespace ScriptEngine.Machine.Contexts
{
    public class ClrEnumValueWrapper<T> : EnumerationValue, IObjectWrapper where T :struct
    {
        private readonly T _realValue;

        public ClrEnumValueWrapper(EnumerationContext owner, T realValue):base(owner)
        {
            _realValue = realValue;
        }

        public object UnderlyingObject
        {
            get
            {
                return _realValue;
            }
        }

        public T UnderlyingValue
        {
            get
            {
                return _realValue;
            }
        }

        public override bool Equals(IValue other)
        {
            if (!(other?.GetRawValue() is ClrEnumValueWrapper<T> otherWrapper))
                return false;

            return UnderlyingValue.Equals(otherWrapper.UnderlyingValue);
        }
    }
}
