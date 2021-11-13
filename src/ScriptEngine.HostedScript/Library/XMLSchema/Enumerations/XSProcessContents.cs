/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [EnumerationType("ОбработкаСодержимогоXS", "XSProcessContents")]
    public enum XSProcessContents
    {
        [EnumItem("Строгая", "Strict")]
        Strict = XmlSchemaContentProcessing.Strict,

        [EnumItem("Пропустить", "Skip")]
        Skip = XmlSchemaContentProcessing.Skip,

        [EnumItem("Слабая", "Lax")]
        Lax = XmlSchemaContentProcessing.Lax,

        [EnumItem("ПустаяСсылка", "EmptyRef")]
        EmptyRef = XmlSchemaContentProcessing.None
    }
}