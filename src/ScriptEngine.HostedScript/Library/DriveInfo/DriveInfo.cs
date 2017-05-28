using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace onescript_extensions.DriveInfo
{
    [ContextClass("ИнформацияОДиске", "DriveInfo")]
    public class DriveInfo : AutoContext<DriveInfo>
    {
        private System.IO.DriveInfo _driveInfo;

        public DriveInfo(string driveName)
        {
            DriveInfo = new System.IO.DriveInfo(driveName);
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue driveName)
        {
            return new DriveInfo(driveName.AsString());
        }


        /// <summary>
        /// Указывает объем доступного свободного места на диске в байтах.
        /// </summary>
        [ContextProperty("Доступно")]
        public Int64 AvailableFreeSpace
        {
            get { return DriveInfo.AvailableFreeSpace; }
        }

        /// <summary>
        /// Получает имя файловой системы
        /// </summary>
        [ContextProperty("ИмяФС")]
        public string DriveFormat
        {
            get { return DriveInfo.DriveFormat; }
        }

        [ContextProperty("ТипДиска", "DriveType")]
        public DriveType DriveTypeProp
        {
            get { return ValueFactory.Create( DriveInfo.DriveType ); }
        }

        /// <summary>
        /// Получает значение, указывающее состояние готовности диска.
        /// </summary>
        [ContextProperty("Готов")]
        public bool IsReady
        {
            get
            {
                return DriveInfo.IsReady;
            }
        }

        /// <summary>
        /// Возвращает имя диска
        /// </summary>
        [ContextProperty("Имя")]
        public string Name
        {
            get
            {
                return DriveInfo.Name;
            }
        }

        /// <summary>
        /// Возвращает корневой каталог диска.
        /// </summary>
        [ContextProperty("КорневойКаталог")]
        public IValue RootDirectory
        {
            get
            {
                return new FileContext(DriveInfo.RootDirectory.FullName);
            }
        }

        /// <summary>
        /// Возвращает общий объем свободного места, доступного на диске, в байтах
        /// </summary>
        [ContextProperty("ОбщийОбъемСвободногоМеста")]
        public Int64 TotalFreeSpace
        {
            get { return DriveInfo.TotalFreeSpace; }
        }

        /// <summary>
        /// Возвращает общий размер места для хранения на диске в байтах.
        /// </summary>
        [ContextProperty("РазмерДиска")]
        public Int64 TotalSize
        {
            get { return DriveInfo.TotalSize; }
        }

        /// <summary>
        /// Возвращает или задает метку тома диска.
        /// </summary>
        [ContextProperty("МеткаТома")]
        public string VolumeLabel
        {
            get
            {
                return DriveInfo.VolumeLabel;
            }
            set
            {
                DriveInfo.VolumeLabel = value;
            }
        }

        public System.IO.DriveInfo DriveInfo
        {
            get
            {
                return _driveInfo;
            }

            set
            {
                _driveInfo = value;
            }
        }
    }
}
