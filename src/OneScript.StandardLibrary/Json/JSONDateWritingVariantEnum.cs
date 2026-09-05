/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Json
{
    [EnumerationType("ВариантЗаписиДатыJSON", "JSONDateWritingVariant",
        TypeUUID = "7B1B8BE8-0EA9-47BF-BAAF-8BC6A4756E8B",
        ValueTypeUUID = "4DD7E419-8D53-4F4C-A3A5-8F7436A07734")]
    public enum JSONDateWritingVariantEnum
    {
        [EnumValue("ЛокальнаяДата", "LocalDate")]
        LocalDate,

        [EnumValue("ЛокальнаяДатаСоСмещением", "LocalDateWithOffset")]
        LocalDateWithOffset,

        [EnumValue("УниверсальнаяДата", "UniversalDate")]
        UniversalDate
    }
}
