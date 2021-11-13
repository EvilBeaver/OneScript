﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Json
{
    [SystemEnum("ФорматДатыJSON", "JSONDateFormat")]
    public class JSONDateFormatEnum : EnumerationContext
    {
        private JSONDateFormatEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue("Microsoft")]
        public EnumerationValue Microsoft => this["Microsoft"];

        [EnumValue("ISO")]
        public EnumerationValue ISO => this["ISO"];

        [EnumValue("JavaScript")]
        public EnumerationValue JavaScript => this["JavaScript"];

        public static JSONDateFormatEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<JSONDateFormatEnum>((t, v) => new JSONDateFormatEnum(t, v));
        }

    }

}
