/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Types;

namespace OneScript.Values
{
    public class BslStringValue : BslPrimitiveValue
    {
        private readonly string _value;
        
        public static BslStringValue Empty { get; } = new BslStringValue(String.Empty);

        public static BslStringValue Create(string value)
        {
            return value == string.Empty ? Empty : new BslStringValue(value);
        }

        private BslStringValue(string value)
        {
            _value = value ?? throw new ArgumentNullException();
        }

        public override TypeDescriptor SystemType => BasicTypes.String;
        
        public override string ToString() => _value;
        
        public static explicit operator decimal(BslStringValue value) => BslNumericValue.Parse(value._value);
        
        public static implicit operator string(BslStringValue value) => value._value;
        
        public static explicit operator DateTime(BslStringValue value) => (DateTime)BslDateValue.Parse(value._value);

        public static explicit operator bool(BslStringValue value) => (bool)BslBooleanValue.Parse(value._value);

        public static string operator +(BslStringValue value, object other) => value._value + other;

        public override bool Equals(BslValue other)
        {
            return other is BslStringValue sv ? _value.Equals(sv._value) : base.Equals(other);
        }

        public override int CompareTo(BslValue other)
        {
            if (ReferenceEquals(null, other))
                return -1;
            
            if (other is BslStringValue s)
                return String.Compare(_value, s._value, StringComparison.OrdinalIgnoreCase);
            if (other is BslUndefinedValue)
                return 1;

            return base.CompareTo(other);
        }
    }
}