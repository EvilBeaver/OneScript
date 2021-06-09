/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Localization;

namespace OneScript.Values
{
    public abstract class BslObjectValue : BslValue
    {
        public override int CompareTo(BslValue other)
        {
            var msg = new BilingualString("Сравнение на больше/меньше для данного типа не поддерживается",
                "Comparision for less/greater is not supported for this type");
            
            throw new RuntimeException(msg);
        }

        public override bool Equals(BslValue other)
        {
            return ReferenceEquals(this, other);
        }
    }
}