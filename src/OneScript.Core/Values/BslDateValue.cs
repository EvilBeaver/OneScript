/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using OneScript.Commons;
using OneScript.Types;

namespace OneScript.Values
{
    public class BslDateValue : BslPrimitiveValue
    {
        private readonly DateTime _value;

        private BslDateValue(DateTime value)
        {
            _value = value;
        }

        public static BslDateValue Create(DateTime value) => new BslDateValue(value);
        
        public override int CompareTo(BslValue other)
        {
            if (ReferenceEquals(null, other))
                return -1;
            
            if(other is BslDateValue d)
                return _value.CompareTo(d._value);

            return base.CompareTo(other);
        }

        public override bool Equals(BslValue other)
        {
            if (other == null)
                return false;

            if(other is BslDateValue d)
                return _value.Equals(d._value);
            
            return base.Equals(other);
        }
        
        public override TypeDescriptor SystemType => BasicTypes.Date;
        
        #region Conversions

        public static explicit operator DateTime(BslDateValue date) => date._value;

        public override string ToString()
        {
            return _value.ToString(CultureInfo.CurrentCulture);
        }

        #endregion

        #region Date Arithmetics

        public static DateTime operator +(BslDateValue left, decimal right)
        {
            return left._value.AddSeconds((double) right);
        }
        
        public static DateTime operator -(BslDateValue left, decimal right)
        {
            return left._value.AddSeconds(-(double) right);
        }
        
        public static decimal operator -(BslDateValue left, DateTime right)
        {
            var span = left._value - right;
            return (decimal) span.TotalSeconds;
        }
        
        public static decimal operator -(BslDateValue left, BslDateValue right)
        {
            var span = left._value - right._value;
            return (decimal) span.TotalSeconds;
        }
        
        #endregion

        public static BslDateValue Parse(string presentation)
        {
            BslDateValue result;
            string format;
            if (presentation.Length == 14)
                format = "yyyyMMddHHmmss";
            else if (presentation.Length == 8)
                format = "yyyyMMdd";
            else if (presentation.Length == 12)
                format = "yyyyMMddHHmm";
            else
                throw BslExceptions.ConvertToDateException();

            if (presentation == "00000000"
                || presentation == "000000000000"
                || presentation == "00000000000000")
            {
                result = new BslDateValue(new DateTime());
            }
            else
            {
                try
                {
                    var date = DateTime.ParseExact(presentation, format,
                        System.Globalization.CultureInfo.InvariantCulture);
                    result = new BslDateValue(date);
                }
                catch (FormatException)
                {
                    throw BslExceptions.ConvertToDateException();
                }
            }

            return result;
        }
    }
}