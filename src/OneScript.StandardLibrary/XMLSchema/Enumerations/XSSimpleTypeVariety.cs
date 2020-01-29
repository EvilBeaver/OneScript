/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("XSSimpleTypeVariety", "ВариантПростогоТипаXS")]
    public enum XSSimpleTypeVariety
    {
        [EnumItem("Atomic", "Атомарная")]
        Atomic,

        [EnumItem("Union", "Объединение")]
        Union,

        [EnumItem("List", "Список")]
        List
    }
}
