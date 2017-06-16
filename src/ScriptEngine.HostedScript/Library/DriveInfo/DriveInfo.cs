using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;

namespace ScriptEngine.HostedScript.Library.DriveInfo
{
    [ContextClass("ИнформацияОДиске", "DriveInfo")]
    public class DriveInfo : AutoContext<DriveInfo>
    {
        private System.IO.DriveInfo _driveInfo;

        public DriveInfo(string driveName)
        {
            SystemDriveInfo = new System.IO.DriveInfo(driveName);
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
            get { return SystemDriveInfo.AvailableFreeSpace; }
        }

        /// <summary>
        /// Получает имя файловой системы
        /// </summary>
        [ContextProperty("ИмяФС")]
        public string DriveFormat
        {
            get { return SystemDriveInfo.DriveFormat; }
        }

        /// <summary>
        /// Возвращает тип диска
        /// </summary>
        /// <value>ТипДиска</value>
        [ContextProperty("ТипДиска", "DriveType")]
        public IValue DriveTypeProp
        {
            get {
                var dte = DriveTypeEnum.CreateInstance();
                return dte.GetPropValue((int)_driveInfo.DriveType);
            }
        }

        /// <summary>
        /// Получает значение, указывающее состояние готовности диска.
        /// </summary>
        [ContextProperty("Готов")]
        public bool IsReady
        {
            get
            {
                return SystemDriveInfo.IsReady;
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
                return SystemDriveInfo.Name;
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
                return new FileContext(SystemDriveInfo.RootDirectory.FullName);
            }
        }

        /// <summary>
        /// Возвращает общий объем свободного места, доступного на диске, в байтах
        /// </summary>
        [ContextProperty("ОбщийОбъемСвободногоМеста")]
        public Int64 TotalFreeSpace
        {
            get { return SystemDriveInfo.TotalFreeSpace; }
        }

        /// <summary>
        /// Возвращает общий размер места для хранения на диске в байтах.
        /// </summary>
        [ContextProperty("РазмерДиска")]
        public Int64 TotalSize
        {
            get { return SystemDriveInfo.TotalSize; }
        }

        /// <summary>
        /// Возвращает или задает метку тома диска.
        /// </summary>
        [ContextProperty("МеткаТома")]
        public string VolumeLabel
        {
            get
            {
                return SystemDriveInfo.VolumeLabel;
            }
            set
            {
                SystemDriveInfo.VolumeLabel = value;
            }
        }

        public System.IO.DriveInfo SystemDriveInfo
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
