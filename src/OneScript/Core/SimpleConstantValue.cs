using System;

namespace OneScript.Core
{
    class SimpleConstantValue : IValue
    {
        private DataType _type;
        private long _integerPart;
        private double _decimalPart;

        public SimpleConstantValue()
        {
            _type = BasicTypes.Undefined;
        }

        public DataType Type
        {
            get { return _type; }
        }

        public double AsNumber()
        {
            if (_type == BasicTypes.Number)
                return _decimalPart;
            else if (_type == BasicTypes.Boolean)
                return AsBoolean() == true ? 1 : 0;
            else
                throw TypeConversionException.ConvertToNumberException();
        }

        public DateTime AsDate()
        {
            if (_type == BasicTypes.Date)
            {
                return new DateTime(_integerPart);
            }
            else
            {
                throw TypeConversionException.ConvertToDateException();
            }
        }

        public bool AsBoolean()
        {
            if (_type == BasicTypes.Boolean)
            {
                return _integerPart != 0;
            }
            else if (_type == BasicTypes.Number)
            {
                return _decimalPart != 0;
            }
            else
            {
                throw TypeConversionException.ConvertToBooleanException();
            }
        }

        public string AsString()
        {
            if (Type == BasicTypes.Number)
            {
                return AsNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            else if (Type == BasicTypes.Date)
            {
                return AsDate().ToString();
            }
            else if (Type == BasicTypes.Boolean)
            {
                return AsBoolean() == true ? "Да" : "Нет";
            }
            else if (Type == BasicTypes.Undefined)
            {
                return "Неопределено";
            }
            else
            {
                return Type.ToString();
            }
        }

        public IRuntimeContextInstance AsObject()
        {
            throw TypeConversionException.ConvertToObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public int CompareTo(IValue other)
        {
            if (other.Type == BasicTypes.Boolean)
                return AsBoolean().CompareTo(other.AsBoolean());
            else if (other.Type == BasicTypes.Date)
                return AsDate().CompareTo(other.AsDate());
            else if (other.Type == BasicTypes.Undefined)
                return other.Type == BasicTypes.Undefined ? 0 : -1;
            else
                return AsNumber().CompareTo(other.AsNumber());
        }


        #region IEquatable<IValue> Members
        public bool Equals(IValue other)
        {
            if (other.Type == this.Type)
            {
                if (Type == BasicTypes.Boolean)
                    return AsBoolean() == other.AsBoolean();
                else if (Type == BasicTypes.Date)
                    return AsDate() == other.AsDate();
                else if (Type == BasicTypes.Number)
                    return AsNumber() == other.AsNumber();
                else if (Type == BasicTypes.Undefined)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public override string ToString()
        {
            return AsString();
        }

        //////////////////////////////////////////////////
        #region Static factory methods

        private static SimpleConstantValue _staticUndef = new SimpleConstantValue();
        private static SimpleConstantValue _staticBoolTrue = BooleanInternal(true);
        private static SimpleConstantValue _staticBoolFalse = BooleanInternal(false);

        public static SimpleConstantValue Undefined()
        {
            return _staticUndef;
        }

        public static SimpleConstantValue Boolean(bool value)
        {
            return value == true ? _staticBoolTrue : _staticBoolFalse;
        }

        private static SimpleConstantValue BooleanInternal(bool value)
        {
            var val = new SimpleConstantValue();
            val._type = BasicTypes.Boolean;
            val._integerPart = value == true ? 1 : 0;

            return val;
        }

        public static SimpleConstantValue Number(double value)
        {
            var val = new SimpleConstantValue();
            val._type = BasicTypes.Number;
            val._decimalPart = value;

            return val;
        }

        public static SimpleConstantValue DateTime(DateTime value)
        {
            var val = new SimpleConstantValue();
            val._type = BasicTypes.Date;
            val._integerPart = value.Ticks;

            return val;
        }

        #endregion

    }
}