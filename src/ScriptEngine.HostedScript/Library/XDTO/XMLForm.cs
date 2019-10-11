﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XDTO
{
    [EnumerationType("XMLForm", "ФормаXML")]
    public enum XMLForm
    {
        [EnumItem("Element", "Элемент")]
        Element,

        [EnumItem("Text", "Текст")]
        Text,

        [EnumItem("Attribute", "Атрибут")]
        Attribute
    }
}
