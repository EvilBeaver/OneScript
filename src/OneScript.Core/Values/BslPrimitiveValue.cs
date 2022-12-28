/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;

namespace OneScript.Values
{
    public abstract class BslPrimitiveValue : BslValue
    {
        public override int CompareTo(BslValue other)
        {
            if (other == null)
                return -1;

            string typeOfThis = null;
            string typeOfOther = null;
            
            try
            {
                typeOfThis = this.SystemType.Name;
                typeOfOther = other.SystemType.Name;
            }
            catch (InvalidOperationException) // если тип не зарегистрирован
            {
                typeOfThis ??= GetType().ToString();
                typeOfOther ??= GetType().ToString();
            }
            
            throw RuntimeException.ComparisonNotSupportedException(typeOfThis, typeOfOther);
        }

        public override bool Equals(BslValue other) => false;
    }
}