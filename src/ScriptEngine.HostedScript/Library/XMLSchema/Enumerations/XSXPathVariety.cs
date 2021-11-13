/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Содержит варианты использования выражения XPath.
    /// </summary>
    /// <see cref="XSXPathDefinition"/>
    [EnumerationType("ВариантXPathXS", "XSXPathVariety")]
    public enum XSXPathVariety
    {
        /// <summary>
        /// Используется в качестве селектора
        /// </summary>
        [EnumItem("Селектор","Selector")]
        Selector,

        /// <summary>
        /// Используется в качестве поля
        /// </summary>
        [EnumItem("Поле","Field")]
        Field
    }
}