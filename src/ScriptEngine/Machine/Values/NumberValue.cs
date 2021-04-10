/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Values
{
    public class NumberValue : BslNumericValue, IValue, IEmptyValueCheck
    {
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

        private NumberValue(decimal value) : base(value)
        {
            DataType = DataType.Number;
        }

        public DateTime AsDate()
        {
            throw RuntimeException.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            return AsNumber() != 0;
        }

        public DataType DataType { get; }

        public TypeDescriptor SystemType => BasicTypes.Number;

        public decimal AsNumber()
        {
            return ActualValue;
        }

        public string AsString()
        {
            return ConvertToString();
        }

        public IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public int CompareTo(IValue other)
        {
            if (other.DataType == DataType.Boolean || other.DataType == DataType.Number)
            {
                return ActualValue.CompareTo(other.AsNumber());
            }

            throw RuntimeException.ComparisonNotSupportedException();
        }

        public bool Equals(IValue other)
        {
            return base.Equals((BslValue)other);
        }

        public bool IsEmpty => ActualValue == 0;
    }
}