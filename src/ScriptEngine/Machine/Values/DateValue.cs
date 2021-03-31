/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Core;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Values
{
    public class DateValue : GenericValue
    {
        private readonly DateTime _value;

        public DateValue(DateTime value)
        {
            _value = value;
        }

        public override DateTime AsDate()
        {
            return _value;
        }

        public override string AsString()
        {
            return _value.ToString();
        }

        public override int CompareTo(IValue other)
        {
            if(other.SystemType == BasicTypes.Date)
                return _value.CompareTo(other.AsDate());

            return base.CompareTo(other);
        }

        public override bool Equals(IValue other)
        {
            if (other == null)
                return false;

            return other.SystemType == BasicTypes.Date && _value.Equals(other.AsDate());
        }

        public override TypeDescriptor SystemType => BasicTypes.Date;

        public override bool IsEmpty => _value.Equals(DateTime.MinValue);
    }
}