using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("РежимВосстановленияПутейФайловZIP", "ZIPRestoreFilePathsMode")]
    public class ZipRestoreFilePathsModeEnum : EnumerationContext
    {
        private const string RESTORE_PATHS_NAME = "Восстанавливать";
        private const string DONT_RESTORE_PATHS_NAME = "НеВосстанавливать";

        private ZipRestoreFilePathsModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(RESTORE_PATHS_NAME)]
        public EnumerationValue Restore
        {
            get
            {
                return this[RESTORE_PATHS_NAME];
            }
        }

        [EnumValue(DONT_RESTORE_PATHS_NAME)]
        public EnumerationValue DoNotRestore
        {
            get
            {
                return this[DONT_RESTORE_PATHS_NAME];
            }
        }

        public static ZipRestoreFilePathsModeEnum CreateInstance()
        {
            ZipRestoreFilePathsModeEnum instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterEnumType<ZipRestoreFilePathsModeEnum>(out enumType, out enumValType);

            instance = new ZipRestoreFilePathsModeEnum(enumType, enumValType);

            EnumContextHelper.RegisterValues<ZipRestoreFilePathsModeEnum>(instance);
            
            return instance;
        }
    }
}
