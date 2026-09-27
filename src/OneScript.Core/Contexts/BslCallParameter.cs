/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Contexts
{
    /// <summary>
    /// Параметр метода в том объеме, который нужен при каждом вызове из 1Script.
    /// </summary>
    public readonly struct BslCallParameter
    {
        public BslCallParameter(bool isByRef, bool hasDefaultValue)
        {
            IsByRef = isByRef;
            HasDefaultValue = hasDefaultValue;
        }

        public bool IsByRef { get; }

        public bool HasDefaultValue { get; }
    }
}
