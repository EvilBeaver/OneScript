/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("XSWhitespaceHandling", "ОбработкаПробельныхСимволовXS")]
    public enum XSWhitespaceHandling
    {
        [EnumItem("Replace", "Заменять")]
        Replace,

        [EnumItem("Collapse", "Сворачивать")]
        Collapse,

        [EnumItem("Preserve", "Сохранять")]
        Preserve
    }
}
