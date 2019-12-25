/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Values
{
    public class NumberValue : GenericValue
    {
        private readonly decimal _value;

        private static readonly NumberValue[] _popularValues = new NumberValue[10];

        static NumberValue()
        {
            for (int i = 0; i < 10; i++)
            {
                _popularValues[i] = new NumberValue(i);
            }
        }

        public static NumberValue Create(decimal value)
        {
            switch (value)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return _popularValues[(int)value];
                default:
                    return new NumberValue(value);
            }
        }

        public static NumberValue Create(double value)
        {
            return Create((decimal)value);
        }

        public static NumberValue Create(int value)
        {
            return Create((decimal)value);
        }

        private NumberValue(decimal value)
        {
            _value = value;
            DataType = DataType.Number;
        }

        public override bool AsBoolean()
        {
            return _value != 0;
        }

        public override decimal AsNumber()
        {
            return _value;
        }

        public override string AsString()
        {
            return AsNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public override int CompareTo(IValue other)
        {
            if (other.DataType == DataType.Boolean || other.DataType == DataType.Number)
            {
                return _value.CompareTo(other.AsNumber());
            }

            return base.CompareTo(other);
        }

        public override bool Equals(IValue other)
        {
            if (other == null)
                return false;

            if (other.DataType == DataType.Number || other.DataType == DataType.Boolean)
                return _value == other.AsNumber();

            return false;
        }
    }
}