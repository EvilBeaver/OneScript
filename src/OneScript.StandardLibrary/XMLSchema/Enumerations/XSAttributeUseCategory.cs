/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("XSAttributeUseCategory", "КатегорияИспользованияАтрибутаXS")]
    public enum XSAttributeUseCategory
    {
        [EnumValue("EmptyRef", "ПустаяСсылка")]
        EmptyRef = XmlSchemaUse.None,

        [EnumValue("Optional", "Необязательно")]
        Optional = XmlSchemaUse.Optional,

        [EnumValue("Prohibited", "Запрещено")]
        Prohibited = XmlSchemaUse.Prohibited,

        [EnumValue("Required", "Обязательно")]
        Required = XmlSchemaUse.Required
    }
}