/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    /// <summary>
    /// Определяет вид модели содержания.
    /// </summary>
    [EnumerationType("МодельСодержимогоXS", "XSContentModel")]
    public enum XSContentModel
    {
        [EnumValue("ПустаяСсылка", "EmptyRef")]
        EmptyRef,

        /// <summary>
        /// Простая модель содержания.
        /// </summary>
        [EnumValue("Простая", "Simple")]
        Simple,

        /// <summary>
        /// Составная модель содержания.
        /// </summary>
        [EnumValue("Составная", "Complex")]
        Complex
    }
}