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
    [EnumerationType("XSProcessContents", "ОбработкаСодержимогоXS")]
    public enum XSProcessContents
    {
        [EnumValue("EmptyRef", "ПустаяСсылка")]
        EmptyRef = XmlSchemaContentProcessing.None,

        [EnumValue("Skip", "Пропустить")]
        Skip = XmlSchemaContentProcessing.Skip,

        [EnumValue("Lax", "Слабая")]
        Lax = XmlSchemaContentProcessing.Lax,

        [EnumValue("Strict", "Строгая")]
        Strict = XmlSchemaContentProcessing.Strict
    }
}