/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;

namespace OneScript.Values
{
    public abstract class BslValue : DynamicObject, IComparable<BslValue>, IEquatable<BslValue>
    {
        protected virtual string ConvertToString() => ToString();
        
        public static explicit operator string(BslValue value)
        {
            return value.ConvertToString();
        }

        public abstract int CompareTo(BslValue other);

        public abstract bool Equals(BslValue other);

        public static explicit operator bool(BslValue target) => (bool) (BslBooleanValue) target;
        
        public static explicit operator decimal(BslValue target) => (decimal) (BslNumericValue) target;
        
        public static explicit operator int(BslValue target) => (int)(decimal)(BslNumericValue) target;
        
        public static explicit operator DateTime(BslValue target) => (DateTime)(BslDateValue) target;
        
    }
}