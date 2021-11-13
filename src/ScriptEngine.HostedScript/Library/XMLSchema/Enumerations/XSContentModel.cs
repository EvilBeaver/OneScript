/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Определяет вид модели содержания.
    /// </summary>
    [EnumerationType("МодельСодержимогоXS", "XSContentModel")]
    public enum XSContentModel
    {
        [EnumItem("Простая", "Simple" )]
        Simple,

        [EnumItem("Составная", "Complex")]
        Complex,

        [EnumItem("ПустаяСсылка", "EmptyRef")]
        EmptyRef
    }
}