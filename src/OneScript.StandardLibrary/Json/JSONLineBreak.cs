/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    [EnumerationType("ПереносСтрокJSON", "JSONLineBreak",
        TypeUUID = "C0049501-11D3-41F8-8FAD-767AE8CD7C7E",
        ValueTypeUUID = "623C2D3A-01B4-43E3-AF2B-9EB922D0D838")]
    public enum JSONLineBreakEnum
    {
        [EnumValue("Авто", "Auto")]
        Auto,
        
        [EnumValue("Unix")]
        Unix,
        
        [EnumValue("Windows")]
        Windows,
        
        [EnumValue("Нет", "None")]
        None
    }

}
