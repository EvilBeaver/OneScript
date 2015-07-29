/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    class CLREnumValueWrapper<T> : EnumerationValue
    {
        T _realValue;

        public CLREnumValueWrapper(EnumerationContext owner, T realValue):base(owner)
        {
            _realValue = realValue;
        }

        public T UnderlyingObject
        {
            get
            {
                return _realValue;
            }
        }

        public override bool Equals(Machine.IValue other)
        {
            var otherWrapper = other.GetRawValue() as CLREnumValueWrapper<T>;
            if (otherWrapper == null)
                return false;

            return this.UnderlyingObject.Equals(otherWrapper.UnderlyingObject);
        }
    }
}
