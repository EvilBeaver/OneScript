/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    /// <summary>
    /// Определяет вид модели содержания.
    /// </summary>
    [EnumerationType("XSContentModel", "МодельСодержимогоXS")]
    public enum XSContentModel
    {
        [EnumItem("EmptyRef", "ПустаяСсылка")]
        EmptyRef,

        [EnumItem("Simple", "Простая")]
        Simple,

        [EnumItem("Complex", "Составная")]
        Complex
    }
}