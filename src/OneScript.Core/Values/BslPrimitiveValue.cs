/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Localization;

namespace OneScript.Values
{
    public abstract class BslPrimitiveValue : BslValue
    {
        public override int CompareTo(BslValue other)
        {
            var typeOfThis = this.GetType();
            var typeOfOther = other.GetType();
            var message = new BilingualString(
                $"{this}({typeOfThis}),{other}({typeOfOther}): Сравнение на больше/меньше для данного типа не поддерживается",
                $"{this}({typeOfThis}),{other}({typeOfOther}): Comparison for greater/less than is not supported for this type");
            
            throw new BslRuntimeException(message);
        }

        public override bool Equals(BslValue other) => false;
    }
}