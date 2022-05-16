/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Text
{
    [EnumerationType("ИспользованиеByteOrderMark", "ByteOrderMarkUsage")]
    public enum ByteOrderMarkUsageEnum
    {
        [EnumValue("Авто", "Auto")]
        Auto,

        [EnumValue("Использовать", "Use")]
        Use,

        [EnumValue("НеИспользовать", "DontUse")]
        DontUse
    }
}
