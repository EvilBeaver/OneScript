/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary.XMLSchema.Objects;
using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    /// <summary>
    /// Содержит варианты использования выражения XPath.
    /// </summary>
    /// <see cref="XSXPathDefinition"/>
    [EnumerationType("XSXPathVariety", "ВариантXPathXS")]
    public enum XSXPathVariety
    {
        /// <summary>
        /// Используется в качестве поля
        /// </summary>
        [EnumItem("Field", "Поле")]
        Field,
  
        /// <summary>
        /// Используется в качестве селектора
        /// </summary>
        [EnumItem("Selector", "Селектор")]
        Selector
    }
}