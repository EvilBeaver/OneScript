/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Json
{
    [SystemEnum("ТипЗначенияJSON", "JSONValueType")]
    public class JSONValueTypeEnum : EnumerationContext
    {
        private JSONValueTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue("НачалоОбъекта", "ObjectStart")]
        public EnumerationValue ObjectStart => this["НачалоОбъекта"];

        [EnumValue("КонецОбъекта", "ObjectEnd")]
        public EnumerationValue ObjectEnd => this["КонецОбъекта"];

        [EnumValue("НачалоМассива", "ArrayStart")]
        public EnumerationValue ArrayStart => this["НачалоМассива"];

        [EnumValue("КонецМассива", "ArrayEnd")]
        public EnumerationValue ArrayEnd => this["КонецМассива"];

        [EnumValue("ИмяСвойства", "PropertyName")]
        public EnumerationValue PropertyName => this["ИмяСвойства"];

        [EnumValue("Число", "Number")]
        public EnumerationValue Number => this["Число"];

        [EnumValue("Строка", "String")]
        public EnumerationValue String => this["Строка"];

        [EnumValue("Булево", "Boolean")]
        public EnumerationValue Boolean => this["Булево"];

        [EnumValue("Null")]
        public EnumerationValue Null => this["Null"];

        [EnumValue("Комментарий", "Comment")]
        public EnumerationValue Comment => this["Комментарий"];

        [EnumValue("Ничего", "None")]
        public EnumerationValue None => this["Ничего"];

        public static JSONValueTypeEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<JSONValueTypeEnum>((t, v) => new JSONValueTypeEnum(t, v));
        }

    }

}
