/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Описывает варианты ограничения значения
    /// </summary>
    [EnumerationType("XSConstraint", "ОграничениеЗначенияXS")]
    public enum XSConstraint
    {
        /// <summary>
        /// Используется ограничение по умолчанию
        /// </summary>
        [EnumItem("Default", "ПоУмолчанию")]
        Default,

        /// <summary>
        /// Используется фиксированное значение
        /// </summary>
        [EnumItem("Fixed", "Фиксированное")]
        Fixed
    }
}
