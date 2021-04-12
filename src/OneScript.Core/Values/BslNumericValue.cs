/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;
using System.Globalization;

namespace OneScript.Values
{
    public class BslNumericValue : BslPrimitiveValue, IEquatable<BslNumericValue>
    {
        private static readonly BslNumericValue[] _popularValues = new BslNumericValue[10];
        static BslNumericValue()
        {
            for (int i = 0; i < 10; i++)
            {
                _popularValues[i] = new BslNumericValue(i);
            }
        }
        
        
        private readonly decimal _value;

        protected BslNumericValue(decimal value)
        {
            _value = value;
        }
        
        public static BslNumericValue Create(decimal value)
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
                    return new BslNumericValue(value);
            }
        }
  
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (!binder.Type.IsNumeric())
            {
                if (binder.Type == typeof(string))
                {
                    result = ConvertToString();
                }
                throw new InvalidOperationException($"Conversion from Number to {binder.Type} is not supported");
            }

            result = Convert.ToDecimal(_value);
            return true;
        }

        public override string ToString()
        {
            return _value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public bool Equals(BslNumericValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value == other._value;
        }

        public override bool Equals(BslValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                BslNumericValue num => Equals(num),
                BslBooleanValue boolean => _value == (decimal) boolean,
                _ => false
            };
        }

        public override int CompareTo(BslValue other)
        {
            return other switch
            {
                BslNumericValue num => _value.CompareTo(num._value),
                BslBooleanValue boolean => _value.CompareTo((decimal) boolean),
                _ => base.CompareTo(other)
            };
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BslNumericValue) obj);
        }

        protected decimal ActualValue => _value;
        
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        
        #region Arithmetics for BslNumeric on Right

        public static decimal operator +(decimal left, BslNumericValue right)
        {
            return left + right._value;
        }
        
        public static decimal operator -(decimal left, BslNumericValue right)
        {
            return left - right._value;
        }
        
        public static decimal operator *(decimal left, BslNumericValue right)
        {
            return left * right._value;
        }
        
        public static decimal operator /(decimal left, BslNumericValue right)
        {
            return left / right._value;
        }
        
        public static decimal operator %(decimal left, BslNumericValue right)
        {
            return left % right._value;
        }

        #endregion
        
        #region Arithmetics for BslNumeric on Left

        public static decimal operator +(BslNumericValue left, decimal right)
        {
            return left._value + right;
        }
        
        public static decimal operator -(BslNumericValue left, decimal right)
        {
            return left._value - right;
        }
        
        public static decimal operator *(BslNumericValue left, decimal right)
        {
            return left._value * right;
        }
        
        public static decimal operator /(BslNumericValue left, decimal right)
        {
            return left._value / right;
        }
        
        public static decimal operator %(BslNumericValue left, decimal right)
        {
            return left._value % right;
        }

        #endregion
        
        #region Arithmetics for both BslNumeric

        public static decimal operator +(BslNumericValue left, BslNumericValue right)
        {
            return left._value + right._value;
        }
        
        public static decimal operator -(BslNumericValue left, BslNumericValue right)
        {
            return left._value - right._value;
        }
        
        public static decimal operator *(BslNumericValue left, BslNumericValue right)
        {
            return left._value * right._value;
        }
        
        public static decimal operator /(BslNumericValue left, BslNumericValue right)
        {
            return left._value / right._value;
        }
        
        public static decimal operator %(BslNumericValue left, BslNumericValue right)
        {
            return left._value % right._value;
        }

        #endregion

        #region Unary operators

        public static decimal operator -(BslNumericValue number)
        {
            return -number._value;
        }
        
        public static decimal operator +(BslNumericValue number)
        {
            return number._value;
        }
        
        public static explicit operator bool(BslNumericValue numVal)
        {
            return numVal._value != 0;
        }
        
        public static implicit operator decimal(BslNumericValue numVal)
        {
            return numVal._value;
        }
        
        public static implicit operator int(BslNumericValue numVal)
        {
            return (int)numVal._value;
        }
        
        public static implicit operator long(BslNumericValue numVal)
        {
            return (long)numVal._value;
        }
        
        #endregion

        public static bool operator ==(BslNumericValue left, BslNumericValue right)
        {
            return left?.Equals(right) ?? ReferenceEquals(right, null);
        }

        public static bool operator !=(BslNumericValue left, BslNumericValue right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return !left.Equals(right);
        }
        
        public static bool operator ==(BslNumericValue left, decimal right)
        {
            if (ReferenceEquals(left, null))
                return right == 0;
            
            return left._value == right;
        }

        public static bool operator !=(BslNumericValue left, decimal right)
        {
            if (ReferenceEquals(left, null))
                return right != 0;

            return left._value != right;
        }

        public static BslNumericValue Parse(string presentation)
        {
            var numInfo = NumberFormatInfo.InvariantInfo;
            var numStyle = NumberStyles.AllowDecimalPoint
                           |NumberStyles.AllowLeadingSign
                           |NumberStyles.AllowLeadingWhite
                           |NumberStyles.AllowTrailingWhite;

            try
            {
                var number = decimal.Parse(presentation, numStyle, numInfo);
                return new BslNumericValue(number);
            }
            catch (FormatException)
            {
                // TODO: сделать исключение правильного Runtime типа
                throw;
                //throw RuntimeException.ConvertToNumberException();
            }
        }
    }
}