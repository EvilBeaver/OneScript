/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
{
    [SystemEnum("РежимОбработкиПодкаталоговZIP", "ZIPSubDirProcessingMode")]
    public class ZIPSubDirProcessingModeEnum : EnumerationContext
    {
        private const string EV_DONT_RECURSE = "НеОбрабатывать";
        private const string EV_RECURSE = "ОбрабатыватьРекурсивно";

        private ZIPSubDirProcessingModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(EV_DONT_RECURSE, "DontProcess")]
        public EnumerationValue DontRecurse
        {
            get
            {
                return this[EV_DONT_RECURSE];
            }
        }

        [EnumValue(EV_RECURSE, "ProcessRecursively")]
        public EnumerationValue Recurse
        {
            get
            {
                return this[EV_RECURSE];
            }
        }

        public static ZIPSubDirProcessingModeEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<ZIPSubDirProcessingModeEnum>((t, v) => new ZIPSubDirProcessingModeEnum(t, v));
        }
    }
}
