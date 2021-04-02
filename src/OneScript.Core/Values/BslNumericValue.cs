/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;
using OneScript.Dynamic;

namespace OneScript.Values
{
    public class BslNumericValue : BslPrimitiveValue, IEquatable<BslNumericValue>
    {
        private readonly decimal _value;

        public BslNumericValue(decimal value)
        {
            _value = value;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            if (!arg.GetType().IsNumeric())
            {
                throw new InvalidOperationException($"Conversion to Number from {arg.GetType()} is not supported");
            }

            result = Convert.ToDecimal(arg);
            return true;
        }

        public bool Equals(BslNumericValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BslNumericValue) obj);
        }

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
        
        #endregion

        public static bool operator ==(BslNumericValue left, BslNumericValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BslNumericValue left, BslNumericValue right)
        {
            return !Equals(left, right);
        }
    }
}