/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Machine.Values
{
    public class StringValue : GenericValue
    {
        public static StringValue Empty { get; } = new StringValue(String.Empty);

        private readonly string _value;

        public static StringValue Create(string value)
        {
            return value == string.Empty ? Empty : new StringValue(value);
        }

        private StringValue(string value)
        {
            _value = value ?? throw new ArgumentNullException();
            DataType = DataType.String;
        }

        public override string AsString()
        {
            return _value;
        }

        public override decimal AsNumber()
        {
            return ValueFactory.Parse(_value, DataType.Number).AsNumber();
        }

        public override DateTime AsDate()
        {
            return ValueFactory.Parse(_value, DataType.Date).AsDate();
        }

        public override bool AsBoolean()
        {
            return ValueFactory.Parse(_value, DataType.Boolean).AsBoolean();
        }

        public override int CompareTo(IValue other)
        {
            if(other?.DataType == DataType.String)
                return String.Compare(_value, other.AsString(), StringComparison.Ordinal);

            throw RuntimeException.ComparisonNotSupportedException();
        }

        public override bool Equals(IValue other)
        {
            if (other?.DataType == DataType)
            {
                var scv = other.AsString();
                return scv == _value;
            }

            return false;
        }
    }
}