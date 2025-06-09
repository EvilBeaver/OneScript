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
    [EnumerationType("ОбработкаСодержимогоXS", "XSProcessContents")]
    public enum XSProcessContents
    {
        [EnumValue("ПустаяСсылка", "EmptyRef")]
        EmptyRef = XmlSchemaContentProcessing.None,

        [EnumValue("Пропустить", "Skip")]
        Skip = XmlSchemaContentProcessing.Skip,

        [EnumValue("Слабая", "Lax")]
        Lax = XmlSchemaContentProcessing.Lax,

        [EnumValue("Строгая", "Strict")]
        Strict = XmlSchemaContentProcessing.Strict
    }
}