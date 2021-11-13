/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [EnumerationType("КатегорияИспользованияАтрибутаXS","XSAttributeUseCategory")]
    public enum XSAttributeUseCategory
    {
        [EnumItem("Необязательно", "Optional")]
        Optional = XmlSchemaUse.Optional,

        [EnumItem("Запрещено", "Prohibited")]
        Prohibited = XmlSchemaUse.Prohibited,

        [EnumItem("Обязательно", "Required")]
        Required = XmlSchemaUse.Required,

        [EnumItem("ПустаяСсылка", "EmptyRef")]
        EmptyRef = XmlSchemaUse.None
    }
}