/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XMLSchema
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
