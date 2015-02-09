using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
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

        [EnumValue(EV_DONT_RECURSE)]
        public EnumerationValue DontRecurse
        {
            get
            {
                return this[EV_DONT_RECURSE];
            }
        }

        [EnumValue(EV_RECURSE)]
        public EnumerationValue Recurse
        {
            get
            {
                return this[EV_RECURSE];
            }
        }

        public static ZIPSubDirProcessingModeEnum CreateInstance()
        {
            ZIPSubDirProcessingModeEnum instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterEnumType<ZIPSubDirProcessingModeEnum>(out enumType, out enumValType);

            instance = new ZIPSubDirProcessingModeEnum(enumType, enumValType);

            EnumContextHelper.RegisterValues<ZIPSubDirProcessingModeEnum>(instance);

            return instance;
        }
    }
}
