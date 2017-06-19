using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.DriveInfo
{
    /// <summary>
    /// Типы дисков:
    /// - Диск является устройством оптических дисков, такие как компакт-ДИСК или DVD-диск.
    /// - Диск является жестким диском.
    /// - Диск является сетевым диском.
    /// - Диск не имеет корневой каталог.
    /// - Диск является диском ОЗУ.
    /// - Диск является съемное запоминающее устройство, например, дисковод гибких дисков или USB-устройство флэш-памяти.
    /// - Тип диска неизвестен.
    /// </summary>
    [SystemEnum("ТипДиска", "DriveType")]
    public class DriveTypeEnum : EnumerationContext
    {

        private DriveTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public static DriveTypeEnum CreateInstance()
        {
            DriveTypeEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеТипДиска", typeof(DriveTypeEnum));
            var enumValueType = TypeManager.RegisterType("ТипДиска", typeof(CLREnumValueWrapper<System.IO.DriveType>));

            instance = new DriveTypeEnum(type, enumValueType);

            instance.AddValue("Неизвестный", "Unknown", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.Unknown));
            instance.AddValue("НеИмеетКорневойКаталог", "NoRootDirectory", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.NoRootDirectory));
            instance.AddValue("СъемноеЗапоминающееУстройство", "Removable", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.Removable));
            instance.AddValue("ЖесткийДиск", "Fixed", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.Fixed));
            instance.AddValue("СетевойДиск", "Network", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.Network));
            instance.AddValue("ОптическийДиск", "CDRom", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.CDRom));
            instance.AddValue("ДискОЗУ", "Ram", new CLREnumValueWrapper<System.IO.DriveType>(instance, System.IO.DriveType.Ram));

            return instance;
        }

    }

}
