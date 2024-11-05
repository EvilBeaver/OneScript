/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.StandardLibrary.Binary
{
    public static class FileBackingConstants
    {
        public const int DEFAULT_MEMORY_LIMIT = 1024 * 1024 * 50; // 50 Mb
        public const int SYSTEM_IN_MEMORY_LIMIT = Int32.MaxValue;
    }
}