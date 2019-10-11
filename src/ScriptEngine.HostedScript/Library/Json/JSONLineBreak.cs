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
    [SystemEnum("ПереносСтрокJSON", "JSONLineBreak")]
    public class JSONLineBreakEnum : EnumerationContext
    {
        private JSONLineBreakEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue("Unix")]
        public EnumerationValue Unix
        {
            get
            {
                return this["Unix"];
            }
        }

        [EnumValue("Windows")]
        public EnumerationValue Windows
        {
            get
            {
                return this["Windows"];
            }
        }

        [EnumValue("Авто", "Auto")]
        public EnumerationValue Auto
        {
            get
            {
                return this["Авто"];
            }
        }

        [EnumValue("Нет", "None")]
        public EnumerationValue None
        {
            get
            {
                return this["Нет"];
            }
        }

        public static JSONLineBreakEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<JSONLineBreakEnum>((t, v) => new JSONLineBreakEnum(t, v));
        }

    }

}
