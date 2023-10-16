﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Класс предоставляет информацию о системе
    /// </summary>
    [ContextClass("СистемнаяИнформация", "SystemInfo")]
    public class SystemEnvironmentContext : AutoContext<SystemEnvironmentContext>
    {

        private static readonly string _osKernelName;
        private static readonly PlatformID _platformId;
        
        /// <summary>
        /// Имя машины, на которой выполняется сценарий
        /// </summary>
        [ContextProperty("ИмяКомпьютера", "MachineName")]
        public string MachineName => Environment.MachineName;

        /// <summary>
        /// Версия операционной системы, на которой выполняется сценарий
        /// </summary>
        [ContextProperty("ВерсияОС", "OSVersion")]
        public string OSVersion => Environment.OSVersion.VersionString;

        /// <summary>
        /// Имя ядра ОС/
        /// </summary>
        [ContextProperty("ИмяЯдра", "KernelName")]
        public string KernelName => _osKernelName; // позволит различать linux/mac/hp-ux/sunos/...

        /// <summary>
        /// Версия OneScript, выполняющая данный сценарий
        /// </summary>
        [ContextProperty("Версия", "Version")]
        public string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var informationVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "<unset>";

                return informationVersion;
            }
        }

        /// <summary>
        /// Тип операционной системы, на которой выполняется сценарий
        /// </summary>
        [ContextProperty("ТипПлатформы", "PlatformType")]
        public PlatformTypeEnum PlatformType
        {
            get
            {
                switch (_platformId) {
                    case PlatformID.Win32NT: return Is64BitOperatingSystem ? PlatformTypeEnum.Windows_x86_64 : PlatformTypeEnum.Windows_x86;
                    case PlatformID.MacOSX: return Is64BitOperatingSystem ? PlatformTypeEnum.MacOS_x86_64 : PlatformTypeEnum.MacOS_x86;
                    case PlatformID.Unix: return Is64BitOperatingSystem ? PlatformTypeEnum.Linux_x86_64 : PlatformTypeEnum.Linux_x86;
                    default: return PlatformTypeEnum.Unknown;
                }
            }
        }

        /// <summary>
        /// Имя пользователя ОС с учетом домена
        /// Формат строки: \\ИмяДомена\ИмяПользователя. 
        /// </summary>
        [ContextProperty("ПользовательОС", "OSUser")]
        public string OSUser
        {
            get
            {
                string DomainName = Environment.UserDomainName;

                if (DomainName != "")
                {
                    return @"\\" + DomainName + @"\" + Environment.UserName;
                }

                return Environment.UserName;
            }
        }

        /// <summary>
        /// Определяет, является ли текущая операционная система 64-разрядной.
        /// </summary>
        [ContextProperty("Это64БитнаяОперационнаяСистема")]
        public bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;

        /// <summary>
        /// Возвращает число процессоров.
        /// 32-битовое целое число со знаком, которое возвращает количество процессоров на текущем компьютере. 
        /// Значение по умолчанию отсутствует. Если текущий компьютер содержит несколько групп процессоров, 
        /// данное свойство возвращает число логических процессоров, доступных для использования средой CLR
        /// </summary>
        [ContextProperty("КоличествоПроцессоров")]
        public int ProcessorCount => Environment.ProcessorCount;

        /// <summary>
        /// Возвращает количество байтов на странице памяти операционной системы
        /// </summary>
        [ContextProperty("РазмерСистемнойСтраницы")]
        public int SystemPageSize => Environment.SystemPageSize;

        /// <summary>
        /// Возвращает время, истекшее с момента загрузки системы (в миллисекундах).
        /// </summary>
        [ContextProperty("ВремяРаботыСМоментаЗагрузки")]
        public long TickCount
        {
            get
            {
                var unsig = (uint)Environment.TickCount;
                return unsig;
            }
        }

        /// <summary>
        /// Возвращает путь для специальной папки. Поддерживаемые значения:
        /// 
        /// * МоиДокументы / MyDocuments            
        /// * ДанныеПриложений / ApplicationData
        /// * ЛокальныйКаталогДанныхПриложений / LocalApplicationData            
        /// * РабочийСтол / Desktop
        /// * КаталогРабочийСтол / DesktopDirectory
        /// * МояМузыка / MyMusic
        /// * МоиРисунки / MyPictures
        /// * Шаблоны / Templates
        /// * МоиВидеозаписи / MyVideos
        /// * ОбщиеШаблоны / CommonTemplates
        /// * ПрофильПользователя / UserProfile
        /// * ОбщийКаталогДанныхПриложения / CommonApplicationData
        /// </summary>
        /// <param name="folder">Тип: СпециальнаяПапка</param>
        /// <returns>Строка</returns>
        [ContextMethod("ПолучитьПутьПапки")]
        public string GetFolderPath(IValue folder)
        {
            var typedValue = folder as ClrEnumValueWrapper<Environment.SpecialFolder>;
            if (typedValue == null)
                throw RuntimeException.InvalidArgumentType();

            return Environment.GetFolderPath(typedValue.UnderlyingValue);
            
        }

        /// <summary>
        /// Возвращает массив строк, содержащий имена логических дисков текущего компьютера.
        /// </summary>
        [ContextProperty("ИменаЛогическихДисков")]
        public FixedArrayImpl GetLogicalDrives
        {
            get
            {
                var arr = new ArrayImpl();
                var data = Environment.GetLogicalDrives();
                foreach (var itm in data)
                {
                    arr.Add(ValueFactory.Create(itm));
                }
                return new FixedArrayImpl(arr);
            }
        }


        /// <summary>
        /// Возвращает соответствие переменных среды. Ключом является имя переменной, а значением - значение переменной
        /// </summary>
        /// <example>
        /// СИ = Новый СистемнаяИнформация();
        /// Для Каждого Переменная Из СИ.ПеременныеСреды() Цикл
        ///     Сообщить(Переменная.Ключ + " = " + Переменная.Значение);
        /// КонецЦикла;
        /// </example>
        /// <returns>Соответствие</returns>
        [ContextMethod("ПеременныеСреды", "EnvironmentVariables")]
        public MapImpl EnvironmentVariables()
        {
            SystemLogger.Write("WARNING! Deprecated method: 'SystemInfo.EnvironmentVariables' is deprecated, use 'EnvironmentVariables' from global context");
            var varsMap = new MapImpl();
            var allVars = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry item in allVars)
            {
                varsMap.Insert(
                    ValueFactory.Create((string)item.Key),
                    ValueFactory.Create((string)item.Value));
            }

            return varsMap;
        }
        
        /// <summary>
        /// Позволяет установить переменную среды. 
        /// Переменная устанавливается в области видимости процесса и очищается после его завершения.
        /// </summary>
        /// <param name="varName">Имя переменной</param>
        /// <param name="value">Значение переменной</param>
        [ContextMethod("УстановитьПеременнуюСреды","SetEnvironmentVariable")]
        public void SetEnvironmentVariable(string varName, string value)
        {
            SystemLogger.Write(string.Format(Locale.NStr("en='{0}';ru='{1}'"),
                "WARNING! Deprecated method: \"SystemInfo.SetEnvironmentVariable\" is deprecated, use \"SetEnvironmentVariable\" from global context",
                "Предупреждение! Устаревший метод: \"СистемнаяИнформация.УстановитьПеременнуюСреды\" устарел, используйте метод глобального контекста \"УстановитьПеременнуюСреды\""));
            Environment.SetEnvironmentVariable(varName, value);
        }

        /// <summary>
        /// Получить значение переменной среды.
        /// </summary>
        /// <param name="varName">Имя переменной</param>
        /// <returns>Строка. Значение переменной</returns>
        [ContextMethod("ПолучитьПеременнуюСреды", "GetEnvironmentVariable")]
        public IValue GetEnvironmentVariable(string varName)
        {
            SystemLogger.Write(string.Format(Locale.NStr("en='{0}';ru='{1}'"),
               "WARNING! Deprecated method: \"SystemInfo.GetEnvironmentVariable\" is deprecated, use \"GetEnvironmentVariable\" from global context",
                "Предупреждение! Устаревший метод: \"СистемнаяИнформация.ПолучитьПеременнуюСреды\" устарел, используйте метод глобального контекста \"ПолучитьПеременнуюСреды\""));
            string value = Environment.GetEnvironmentVariable(varName);
            if (value == null)
                return ValueFactory.Create();
            return ValueFactory.Create(value);

        }

        [ScriptConstructor]
        public static SystemEnvironmentContext Create()
        {
            return new SystemEnvironmentContext();
        }

        static SystemEnvironmentContext()
        {
            _platformId = Environment.OSVersion.Platform;
            switch (_platformId)
            {
                case PlatformID.Unix:
                    _osKernelName = SystemHelper.UnixKernelName();
                    if (_osKernelName == "Darwin")
                    {
                        _platformId = PlatformID.MacOSX;
                    }
                    break;
                case PlatformID.Win32NT:
                    _osKernelName = "WindowsNT";
                    break;
                case PlatformID.MacOSX:
                    _osKernelName = "Darwin";
                    break;
            }
        }
    }
}
