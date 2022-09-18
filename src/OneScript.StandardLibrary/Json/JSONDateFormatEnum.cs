/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Json
{
    [EnumerationType("ФорматДатыJSON", "JSONDateFormat",
        TypeUUID = "CDBA878E-FF5C-40A5-ADF3-E473176812BD",
        ValueTypeUUID = "29E40EE6-57C7-4E5B-98D8-9B5DDE8CF667")]
    public enum JSONDateFormatEnum
    {
        [EnumValue("ISO")]
        ISO,
        [EnumValue("JavaScript")]
        JavaScript,
        [EnumValue("Microsoft")]
        Microsoft
    }
}
