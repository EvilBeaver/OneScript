using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptEngine.Machine;

namespace onescript_extensions.DriveInfo
{
    [SystemEnum("ТипДиска", "DriveType")]
    class DriveTypeEnum : EnumerationContext
    {

        const System.IO.DriveType CDRom = System.IO.DriveType.CDRom;
        //const int SHA1 = "SHA1";
        //const int SHA256 = "SHA256";
        //const int SHA384 = "SHA384";
        //const int SHA512 = "SHA512";
        //const int CRC32 = "CRC32";

        //public DriveTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) : base(typeRepresentation, valuesType)
        //{
        //}

        /// <summary>
        /// Диск является устройством оптических дисков, такие как компакт-ДИСК или DVD-диск.
        /// </summary>
        [ContextProperty("ОптическийДиск")]
        public int CDRom
        {
            get { return (int)System.IO.DriveType.CDRom; }
        }

        /// <summary>
        /// Диск является жестким диском.
        /// </summary>
        [ContextProperty("ЖесткийДиск")]
        public int Fixed
        {
            get { return (int)System.IO.DriveType.Fixed; }
        }

        /// <summary>
        /// Диск является сетевым диском.
        /// </summary>
        [ContextProperty("СетевойДиск")]
        public int Network
        {
            get { return (int)System.IO.DriveType.Network; }
        }

        /// <summary>
        /// Диск не имеет корневой каталог.
        /// </summary>
        [ContextProperty("НеИмеетКорневойКаталог")]
        public int NoRootDirectory
        {
            get { return (int)System.IO.DriveType.NoRootDirectory; }
        }

        /// <summary>
        /// Диск является диском ОЗУ.
        /// </summary>
        [ContextProperty("ДискОЗУ")]
        public int Ram
        {
            get { return (int)System.IO.DriveType.Ram; }
        }

        /// <summary>
        /// Диск является съемное запоминающее устройство, например, дисковод гибких дисков или USB-устройство флэш-памяти.
        /// </summary>
        [ContextProperty("СъемноеЗапоминающееУстройство")]
        public int Removable
        {
            get { return (int)System.IO.DriveType.Removable; }
        }

        /// <summary>
        /// Тип диска неизвестен.
        /// </summary>
        [ContextProperty("Неизвестный")]
        public int Unknown
        {
            get { return (int)System.IO.DriveType.Unknown; }
        }

    }
}
