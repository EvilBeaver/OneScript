/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Json
{
    [SystemEnum("ТипЗначенияJSON", "JSONValueType")]
    public class JSONValueTypeEnum : EnumerationContext
    {
        private JSONValueTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue("Null")]
        public EnumerationValue Null
        {
            get
            {
                return this["Null"];
            }
        }

        [EnumValue("Булево", "Boolean")]
        public EnumerationValue Boolean
        {
            get
            {
                return this["Булево"];
            }
        }

        [EnumValue("ИмяСвойства", "PropertyName")]
        public EnumerationValue PropertyName
        {
            get
            {
                return this["ИмяСвойства"];
            }
        }

        [EnumValue("Комментарий", "Comment")]
        public EnumerationValue Comment
        {
            get
            {
                return this["Комментарий"];
            }
        }

        [EnumValue("КонецМассива", "ArrayEnd")]
        public EnumerationValue ArrayEnd
        {
            get
            {
                return this["КонецМассива"];
            }
        }

        [EnumValue("КонецОбъекта", "ObjectEnd")]
        public EnumerationValue ObjectEnd
        {
            get
            {
                return this["КонецОбъекта"];
            }
        }

        [EnumValue("НачалоМассива", "ArrayStart")]
        public EnumerationValue ArrayStart
        {
            get
            {
                return this["НачалоМассива"];
            }
        }

        [EnumValue("НачалоОбъекта", "ObjectStart")]
        public EnumerationValue ObjectStart
        {
            get
            {
                return this["НачалоОбъекта"];
            }
        }

        [EnumValue("Ничего", "None")]
        public EnumerationValue None
        {
            get
            {
                return this["Ничего"];
            }
        }

        [EnumValue("Строка", "String")]
        public EnumerationValue String
        {
            get
            {
                return this["Строка"];
            }
        }

        [EnumValue("Число", "Number")]
        public EnumerationValue Number
        {
            get
            {
                return this["Число"];
            }
        }

        public static JSONValueTypeEnum CreateInstance(ITypeManager typeManager)
        {
            return EnumContextHelper.CreateEnumInstance<JSONValueTypeEnum>(typeManager,
                (t, v) => new JSONValueTypeEnum(t, v));
        }

    }

}
