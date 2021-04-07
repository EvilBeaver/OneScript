/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Localization;

namespace OneScript.Values
{
    public class BslBooleanValue : BslPrimitiveValue
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
        
        public static explicit operator decimal(BslBooleanValue value)
        {
            return value._flag ? 1 : 0;
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

            return other.Equals(this);
        }
    }
}