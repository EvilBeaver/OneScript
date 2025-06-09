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
    [EnumerationType("КатегорияИспользованияАтрибутаXS", "XSAttributeUseCategory")]
    public enum XSAttributeUseCategory
    {
        [EnumValue("ПустаяСсылка", "EmptyRef")]
        EmptyRef = XmlSchemaUse.None,

        [EnumValue("Необязательно", "Optional")]
        Optional = XmlSchemaUse.Optional,

        [EnumValue("Запрещено", "Prohibited")]
        Prohibited = XmlSchemaUse.Prohibited,

        [EnumValue("Обязательно", "Required")]
        Required = XmlSchemaUse.Required
    }
}