/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("XSProcessContents", "ОбработкаСодержимогоXS")]
    public enum XSProcessContents
    {
        [EnumItem("EmptyRef", "ПустаяСсылка")]
        EmptyRef = XmlSchemaContentProcessing.None,

        [EnumItem("Skip", "Пропустить")]
        Skip = XmlSchemaContentProcessing.Skip,

        [EnumItem("Lax", "Слабая")]
        Lax = XmlSchemaContentProcessing.Lax,

        [EnumItem("Strict", "Строгая")]
        Strict = XmlSchemaContentProcessing.Strict
    }
}