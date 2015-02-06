using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("РежимВосстановленияПутейФайловZIP", "ZIPRestoreFilePathsMode")]
    public class ZipStorePathModeEnum : EnumerationContext
    {
        const string DONT_SAVE = "НеСохранять";
        const string SAVE_RELATIVE = "СохранятьОтносительныеПути";
        const string SAVE_FULL = "СохранятьПолныеПути";

        public ZipStorePathModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public SelfAwareEnumValue<ZipStorePathModeEnum> DontStorePath
        {
            get
            {
                return (SelfAwareEnumValue<ZipStorePathModeEnum>)this[DONT_SAVE];
            }
        }

        public SelfAwareEnumValue<ZipStorePathModeEnum> StoreRelativePath
        {
            get
            {
                return (SelfAwareEnumValue<ZipStorePathModeEnum>)this[SAVE_RELATIVE];
            }
        }

        public SelfAwareEnumValue<ZipStorePathModeEnum> StoreFullPath
        {
            get
            {
                return (SelfAwareEnumValue<ZipStorePathModeEnum>)this[SAVE_FULL];
            }
        }

        public static ZipStorePathModeEnum CreateInstance()
        {
            ZipStorePathModeEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеРежимВосстановленияПутейФайловZIPXML", typeof(ZipStorePathModeEnum));
            var enumValueType = TypeManager.RegisterType("РежимВосстановленияПутейФайловZIP", typeof(SelfAwareEnumValue<ZipStorePathModeEnum>));

            instance = new ZipStorePathModeEnum(type, enumValueType);

            instance.AddValue(DONT_SAVE, new SelfAwareEnumValue<ZipStorePathModeEnum>(instance, 0));
            instance.AddValue(SAVE_RELATIVE, new SelfAwareEnumValue<ZipStorePathModeEnum>(instance, 1));
            instance.AddValue(SAVE_FULL, new SelfAwareEnumValue<ZipStorePathModeEnum>(instance, 2));

            return instance;
        }
    }
}
