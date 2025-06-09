/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("ОбработкаПробельныхСимволовXS", "XSWhitespaceHandling")]
    public enum XSWhitespaceHandling
    {
        [EnumValue("Заменять", "Replace")]
        Replace,

        [EnumValue("Сворачивать", "Collapse")]
        Collapse,

        [EnumValue("Сохранять", "Preserve")]
        Preserve
    }
}
