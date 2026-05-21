/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.StandardLibrary.Binary
{
    public static class BinaryDataConstants
    {
        /// <summary>
        /// Максимальный размер массива, доступный в среде выполнения.
        /// Де-факто он чуть меньше 2Гб, он же Int32.MaxValue, поэтому используется системная константа <see cref="Array.MaxLength"/>
        /// </summary>
        public static readonly int SYSTEM_IN_MEMORY_LIMIT = Array.MaxLength;
        
        /// <summary>
        /// Размер двоичных данных, хранимый в памяти по умолчанию.
        /// </summary>
        public const int DEFAULT_IN_MEMORY_LIMIT = 1024 * 1024 * 50;
    }
}