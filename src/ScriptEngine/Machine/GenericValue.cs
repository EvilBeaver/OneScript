/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class SimpleConstantValue : IValue
    {
        private DataType _type;
        private long _integerPart;
        private decimal _decimalPart;

        public DataType DataType
        {
            get { return _type; }
        }

        public TypeDescriptor SystemType
        {
            get
            {
                return TypeDescriptor.FromDataType(_type);
            }
        }

        public decimal AsNumber()
        {
            if (_type == Machine.DataType.Number)
                return _decimalPart;
            else if (_type == Machine.DataType.Boolean)
                return AsBoolean() == true ? 1 : 0;
            else
                throw RuntimeException.ConvertToNumberException();
        }

        public DateTime AsDate()
        {
            if (_type == Machine.DataType.Date)
            {
                return new DateTime(_integerPart);
            }
            else
            {
                throw RuntimeException.ConvertToDateException();
            }
        }

        public bool AsBoolean()
        {
            if (_type == Machine.DataType.Boolean)
            {
                return _integerPart != 0;
            }
            else if (_type == Machine.DataType.Number)
            {
                return _decimalPart != 0;
            }
            else
            {
                throw RuntimeException.ConvertToBooleanException();
            }
        }

        public string AsString()
        {
            if(DataType == Machine.DataType.Number)
            {
                return AsNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            else if (DataType == Machine.DataType.Date)
            {
                return AsDate().ToString();
            }
            else if (DataType == Machine.DataType.Boolean)
            {
                return AsBoolean() == true ? "Да" : "Нет";
            }
            else if (DataType == Machine.DataType.Undefined)
            {
                return "";
            }
            else
            {
                return DataType.ToString();
            }
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
            if (other.DataType != this.DataType)
            {
                if (this.DataType == DataType.Boolean && other.DataType == DataType.Number
                    || this.DataType == DataType.Number && other.DataType == DataType.Boolean)
                {
                    return AsNumber().CompareTo(other.AsNumber());
                }
                else
                {
                    throw RuntimeException.ComparisonNotSupportedException();
                }
            }
            
            switch (other.DataType)
            {
                case Machine.DataType.Boolean:
                    return AsBoolean().CompareTo(other.AsBoolean());
                case Machine.DataType.Date:
                    return AsDate().CompareTo(other.AsDate());
                case Machine.DataType.Undefined:
                    return 0;
                default:
                    return AsNumber().CompareTo(other.AsNumber());
            }
        }

        public override string ToString()
        {
            return AsString();
        }

        //////////////////////////////////////////////////
        #region Static factory methods

        private static readonly SimpleConstantValue _staticUndef = new SimpleConstantValue();
        private static readonly SimpleConstantValue _staticBoolTrue = BooleanInternal(true);
        private static readonly SimpleConstantValue _staticBoolFalse = BooleanInternal(false);
        private static readonly SimpleConstantValue _staticIntZero = new SimpleConstantValue() {_decimalPart = 0, _type = DataType.Number};
        private static readonly SimpleConstantValue _staticIntOne = new SimpleConstantValue() {_decimalPart = 1, _type = DataType.Number};

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
            val._type = DataType.Boolean;
            val._integerPart = value == true ? 1 : 0;

            return val;
        }

        public static SimpleConstantValue Number(decimal value)
        {
            if (value == 0)
                return _staticIntZero;

            if (value == 1)
                return _staticIntOne;

            var val = new SimpleConstantValue();
            val._type = DataType.Number;
            val._decimalPart = value;

            return val;
        }

        public static SimpleConstantValue DateTime(DateTime value)
        {
            var val = new SimpleConstantValue();
            val._type = DataType.Date;
            val._integerPart = value.Ticks;

            return val;
        }

        #endregion


        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            if (other.DataType == this.DataType)
            {
                switch (DataType)
                {
                    case Machine.DataType.Number:
                        return this.AsNumber() == other.AsNumber();
                    case Machine.DataType.Boolean:
                        return this.AsBoolean() == other.AsBoolean();
                    case Machine.DataType.Date:
                        return this.AsDate() == other.AsDate();
                    case Machine.DataType.Undefined:
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion
    }

    class StringConstantValue : IValue
    {
        private readonly string _value;

        public StringConstantValue(string val)
        {
            Trace.Assert(val != null);
            _value = val;
        }

        public override string ToString()
        {
            return _value;
        }

        #region IValue Members

        public DataType DataType
        {
            get { return DataType.String; }
        }

        public TypeDescriptor SystemType
        {
            get
            {
                return TypeDescriptor.FromDataType(DataType.String);
            }
        }

        public decimal AsNumber()
        {
            return ValueFactory.Parse(_value, DataType.Number).AsNumber();
        }

        public DateTime AsDate()
        {
            return ValueFactory.Parse(_value, DataType.Date).AsDate();
        }

        public bool AsBoolean()
        {
            return ValueFactory.Parse(_value, DataType.Boolean).AsBoolean();
        }

        public string AsString()
        {
            return _value;
        }

        public IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
        {
            if(other.DataType == DataType.String)
                return _value.CompareTo(other.AsString());

            throw RuntimeException.ComparisonNotSupportedException();
        }

        #endregion

        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            if (other.DataType == this.DataType)
            {
                var scv = other.AsString();
                return scv == this._value;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
