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

namespace ScriptEngine.Machine.Contexts
{
    public class CLREnumValueWrapper<T> : EnumerationValue, IObjectWrapper where T :struct
    {
        private readonly T _realValue;
        private DataType _redefinedDataType;

        public CLREnumValueWrapper(EnumerationContext owner, T realValue):base(owner)
        {
            _realValue = realValue;
            _redefinedDataType = DataType.GenericValue;
        }

        public CLREnumValueWrapper (EnumerationContext owner, T realValue, DataType newDataType) : base (owner)
        {
            _realValue = realValue;
            _redefinedDataType = newDataType;
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

        public override DataType DataType
        {
            get
            {
                return _redefinedDataType;
            }
         }

        public override bool Equals(IValue other)
        {
            var otherWrapper = other.GetRawValue() as CLREnumValueWrapper<T>;
            if (otherWrapper == null)
                return false;

            return UnderlyingValue.Equals(otherWrapper.UnderlyingValue);
        }
    }
}
