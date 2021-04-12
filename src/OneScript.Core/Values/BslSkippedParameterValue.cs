/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;

namespace OneScript.Values
{
    public class BslSkippedParameterValue : BslPrimitiveValue
    {
        public static BslSkippedParameterValue Instance { get; } = new BslSkippedParameterValue();

        private BslSkippedParameterValue()
        {
        }

        public override bool Equals(BslValue other)
        {
            return ReferenceEquals(Instance, other);
        }

        public override string ToString() => string.Empty;
    }
}