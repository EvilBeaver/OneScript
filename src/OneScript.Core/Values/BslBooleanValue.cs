/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Exceptions;
using OneScript.Localization;
using OneScript.Types;

namespace OneScript.Values
{
    public sealed class BslBooleanValue : BslPrimitiveValue
    {
        public static readonly BslBooleanValue True = new BslBooleanValue(true);
        public static readonly BslBooleanValue False = new BslBooleanValue(false);

        private static readonly BilingualString _stringTrue = new BilingualString("Да", "True");
        private static readonly BilingualString _stringFalse = new BilingualString("Нет", "False");
        
        private readonly bool _flag;
        
        private BslBooleanValue(bool flag)
        {
            _flag = flag;
        }
        
        public override TypeDescriptor SystemType => BasicTypes.Boolean;
        
        public static explicit operator decimal(BslBooleanValue value)
        {
            return value._flag ? 1 : 0;
        }

        public static explicit operator bool(BslBooleanValue value) => value._flag;

        public static BslBooleanValue Parse(string presentation)
        {
            if (String.Compare(presentation, "истина", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(presentation, "true", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(presentation, "да", StringComparison.OrdinalIgnoreCase) == 0)
                return True;
            else if (String.Compare(presentation, "ложь", StringComparison.OrdinalIgnoreCase) == 0
                     || String.Compare(presentation, "false", StringComparison.OrdinalIgnoreCase) == 0
                     || String.Compare(presentation, "нет", StringComparison.OrdinalIgnoreCase) == 0)
                return False;
            else
                throw BslExceptions.ConvertToBooleanException();
        }
        
        public override string ToString()
        {
            return _flag? _stringTrue.ToString() : _stringFalse.ToString();
        }
        
        public override bool Equals(BslValue other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return false;
        }

        public override int CompareTo(BslValue other)
        {
            if (other is BslNumericValue || other is BslBooleanValue)
            {
                return ((decimal)this).CompareTo((decimal)other);
            }

            return base.CompareTo(other);
        }

        public static BslValue Create(bool boolean)
        {
            return boolean ? True : False;
        }
    }
}