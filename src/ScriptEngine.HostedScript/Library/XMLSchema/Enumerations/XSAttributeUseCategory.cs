/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [EnumerationType("XSAttributeUseCategory", "КатегорияИспользованияАтрибутаXS")]
    public enum XSAttributeUseCategory
    {
        [EnumItem("EmptyRef", "ПустаяСсылка")]
        EmptyRef = XmlSchemaUse.None,

        [EnumItem("Optional", "Необязательно")]
        Optional = XmlSchemaUse.Optional,

        [EnumItem("Prohibited", "Запрещено")]
        Prohibited = XmlSchemaUse.Prohibited,

        [EnumItem("Required", "Обязательно")]
        Required = XmlSchemaUse.Required
    }
}