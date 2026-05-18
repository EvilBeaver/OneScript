/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.BinaryData
{
    /// <summary>
    /// Реализация <see cref="IBinaryDataMemoryLimit"/> с фиксированным числом байт.
    /// </summary>
    public sealed class BinaryDataMemoryLimit : IBinaryDataMemoryLimit
    {
        public BinaryDataMemoryLimit(int maxBytesInMemory)
        {
            MaxBytesInMemory = maxBytesInMemory;
        }

        public int MaxBytesInMemory { get; }
    }
}
