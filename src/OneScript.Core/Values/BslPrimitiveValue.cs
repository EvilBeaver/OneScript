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
            var message = new BilingualString(
                "Сравнение на больше/меньше для данного типа не поддерживается",
                "Comparison for greater/less than is not supported for this type");
            
            throw new BslCoreException(message);
        }

        public override bool Equals(BslValue other) => false;
    }
}