/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("МетодНаследованияXS", "XSDerivationMethod")]
    public enum XSDerivationMethod
    {
        [EnumValue("ПустаяСсылка", "EmptyRef")]
        EmptyRef,

        [EnumValue("Ограничение", "Restriction")]
        Restriction,

        [EnumValue("Расширение", "Extension")]
        Extension
    }
}