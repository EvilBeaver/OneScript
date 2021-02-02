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
    [SystemEnum("ФорматДатыJSON", "JSONDateFormat")]
    public class JSONDateFormatEnum : EnumerationContext
    {
        private JSONDateFormatEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue("ISO")]
        public EnumerationValue ISO
        {
            get
            {
                return this["ISO"];
            }
        }

        [EnumValue("JavaScript")]
        public EnumerationValue JavaScript
        {
            get
            {
                return this["JavaScript"];
            }
        }

        [EnumValue("Microsoft")]
        public EnumerationValue Microsoft
        {
            get
            {
                return this["Microsoft"];
            }
        }

        public static JSONDateFormatEnum CreateInstance(ITypeManager typeManager)
        {
            return EnumContextHelper.CreateSelfAwareEnumInstance(typeManager, 
                (t, v) => new JSONDateFormatEnum(t, v));
        }

    }

}
