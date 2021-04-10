/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Localization;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    internal static class DynamicOperations
    {
        public static BslValue Add(BslValue left, BslValue right)
        {
            if (left is BslStringValue str)
                return BslStringValue.Create(str + right);
            
            if (left is BslDateValue bslDate && right is BslNumericValue num)
            {
                return BslDateValue.Create(bslDate - (decimal) num);
            }
            
            dynamic dLeft = left;
            dynamic dRight = right;
            return dLeft + dRight;
        }

        public static BslValue Subtract(BslValue left, BslValue right)
        {
            if (left is BslNumericValue num)
            {
                var result = num - ToNumber(right);
                return BslNumericValue.Create(result);
            }
            else if (left is BslDateValue date)
            {
                switch (right)
                {
                    case BslNumericValue numRight:
                    {
                        var result = date - numRight;
                        return BslDateValue.Create(result);
                    }
                    case BslDateValue dateRight:
                    {
                        var result = date - dateRight;
                        return BslNumericValue.Create(result);
                    }
                }
            }
            else
            {
                dynamic dLeft = left;
                dynamic dRight = right;
                return dLeft - dRight;
            }

            throw BslExceptions.ConvertToNumberException();
        }
        
        public static bool ToBoolean(BslValue value)
        {
            return value switch
            {
                BslNumericValue n => (bool)n,
                BslBooleanValue b => (bool)b,
                _ => (bool)(dynamic)value
            };
        }
        
        public static decimal ToNumber(BslValue value)
        {
            return value switch
            {
                BslNumericValue n => (decimal)n,
                BslBooleanValue b => (decimal)b,
                _ => (decimal)(dynamic)value
            };
        }
        
        public static DateTime ToDate(BslValue value)
        {
            return value switch
            {
                BslDateValue n => (DateTime)n,
                _ => (DateTime)(dynamic)value
            };
        }

        public static BslValue WrapToValue(object value)
        {
            return value switch
            {
                null => BslUndefinedValue.Instance,
                string s => BslStringValue.Create(s),
                decimal d => BslNumericValue.Create(d),
                int n => BslNumericValue.Create(n),
                long l => BslNumericValue.Create(l),
                double dbl => BslNumericValue.Create((decimal) dbl),
                bool boolean => BslBooleanValue.Create(boolean),
                DateTime date => BslDateValue.Create(date),
                _ => throw new TypeConversionException(new BilingualString(
                    $"Невозможно преобразовать {value.GetType()} в тип {nameof(BslValue)}",
                    $"Can't Convert {value.GetType()} to {nameof(BslValue)}"))
            };
        }
    }
}