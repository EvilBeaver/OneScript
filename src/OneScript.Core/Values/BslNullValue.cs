/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Types;

namespace OneScript.Values
{
    public class BslNullValue : BslPrimitiveValue
    {
        public static BslNullValue Instance { get; } = new BslNullValue();

        private BslNullValue()
        {
        }

        public override TypeDescriptor SystemType => BasicTypes.Null;
        
        public override bool Equals(BslValue other)
        {
            return ReferenceEquals(Instance, other);
        }

        public override string ToString() => string.Empty;
    }
}