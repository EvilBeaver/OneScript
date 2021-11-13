/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("РежимОбработкиПодкаталоговZIP", "ZIPSubDirProcessingMode")]
    public class ZIPSubDirProcessingModeEnum : EnumerationContext
    {
        private const string EV_RECURSE = "ОбрабатыватьРекурсивно";
        private const string EV_DONT_RECURSE = "НеОбрабатывать";

        private ZIPSubDirProcessingModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(EV_RECURSE, "ProcessRecursively")]
        public EnumerationValue Recurse => this[EV_RECURSE];

        [EnumValue(EV_DONT_RECURSE, "DontProcess")]
        public EnumerationValue DontRecurse => this[EV_DONT_RECURSE];

        public static ZIPSubDirProcessingModeEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<ZIPSubDirProcessingModeEnum>((t, v) => new ZIPSubDirProcessingModeEnum(t, v));
        }
    }
}
