/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    [EnumerationType("ТипЗначенияJSON", "JSONValueType",
        TypeUUID = "90EC0979-11CF-4688-972B-3530FA16EEF5",
        ValueTypeUUID = "D6089B59-1730-452C-B142-C2EF6C75299C")]
    public enum JSONValueTypeEnum
    {
        [EnumValue("Ничего", "None")]
        None,
        
        [EnumValue("Null")]
        Null,
        
        [EnumValue("Булево", "Boolean")]
        Boolean,
        
        [EnumValue("ИмяСвойства", "PropertyName")]
        PropertyName,
        
        [EnumValue("Комментарий", "Comment")]
        Comment,
        
        [EnumValue("КонецМассива", "ArrayEnd")]
        ArrayEnd,
        
        [EnumValue("КонецОбъекта", "ObjectEnd")]
        ObjectEnd,
        
        [EnumValue("НачалоМассива", "ArrayStart")]
        ArrayStart,
        
        [EnumValue("НачалоОбъекта", "ObjectStart")]
        ObjectStart,
        
        [EnumValue("Строка", "String")]
        String,
        
        [EnumValue("Число", "Number")]
        Number
    }
}
