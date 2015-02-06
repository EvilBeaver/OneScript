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

        public SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> Restore
        {
            get
            {
                return (SelfAwareEnumValue<ZipRestoreFilePathsModeEnum>)this[RESTORE_PATHS_NAME];
            }
        }

        public SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> DoNotRestore
        {
            get
            {
                return (SelfAwareEnumValue<ZipRestoreFilePathsModeEnum>)this[DONT_RESTORE_PATHS_NAME];
            }
        }

        public static ZipRestoreFilePathsModeEnum CreateInstance()
        {
            ZipRestoreFilePathsModeEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеРежимВосстановленияПутейФайловZIP", typeof(ZipRestoreFilePathsModeEnum));
            var enumValueType = TypeManager.RegisterType("РежимВосстановленияПутейФайловZIP", typeof(SelfAwareEnumValue<ZipRestoreFilePathsModeEnum>));

            instance = new ZipRestoreFilePathsModeEnum(type, enumValueType);

            instance.AddValue(RESTORE_PATHS_NAME, new SelfAwareEnumValue<ZipRestoreFilePathsModeEnum>(instance));
            instance.AddValue(DONT_RESTORE_PATHS_NAME, new SelfAwareEnumValue<ZipRestoreFilePathsModeEnum>(instance));

            return instance;
        }
    }
}
