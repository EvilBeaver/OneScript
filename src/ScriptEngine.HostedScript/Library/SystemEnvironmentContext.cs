/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Collections;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Класс предоставляет информацию о системе
    /// </summary>
    [ContextClass("СистемнаяИнформация", "SystemInfo")]
    public class SystemEnvironmentContext : AutoContext<SystemEnvironmentContext>
    {
        /// <summary>
        /// Имя машины, на которой выполняется сценарий
        /// </summary>
        [ContextProperty("ИмяКомпьютера", "MachineName")]
        public string MachineName 
        {
            get
            {
                return System.Environment.MachineName;
            }
        }

        /// <summary>
        /// Версия операционной системы, на которой выполняется сценарий
        /// </summary>
        [ContextProperty("ВерсияОС", "OSVersion")]
        public string OSVersion
        {
            get
            {
                return System.Environment.OSVersion.VersionString;
            }
        }

        /// <summary>
        /// Версия OneScript, выполняющая данный сценарий
        /// </summary>
        [ContextProperty("Версия","Version")]
        public string Version 
        { 
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
                string DomainName = System.Environment.UserDomainName;

                if (DomainName != "")
                {
                    return @"\\" + DomainName + @"\" + System.Environment.UserName;
                }

                return System.Environment.UserName;
            }
        }

        /// <summary>
        /// Определяет, является ли текущая операционная система 64-разрядной.
        /// </summary>
        [ContextProperty("Это64БитнаяОперационнаяСистема")]
        public bool Is64BitOperatingSystem
        {
            get { return System.Environment.Is64BitOperatingSystem; }
        }

        /// <summary>
        /// Возвращает число процессоров.
        /// 32-битовое целое число со знаком, которое возвращает количество процессоров на текущем компьютере. 
        /// Значение по умолчанию отсутствует. Если текущий компьютер содержит несколько групп процессоров, 
        /// данное свойство возвращает число логических процессоров, доступных для использования средой CLR
        /// </summary>
        [ContextProperty("КоличествоПроцессоров")]
        public int ProcessorCount
        {
            get { return System.Environment.ProcessorCount; }
        }

        /// <summary>
        /// Возвращает количество байтов на странице памяти операционной системы
        /// </summary>
        [ContextProperty("РазмерСистемнойСтраницы")]
        public int SystemPageSize
        {
            get { return System.Environment.SystemPageSize; }
        }

        /// <summary>
        /// Возвращает время, истекшее с момента загрузки системы (в миллисекундах).
        /// </summary>
        [ContextProperty("ВремяРаботыСМоментаЗагрузки")]
        public int TickCount
        {
            get { return System.Environment.TickCount; }
        }


        ///// <summary>
        ///// Список специальных папок
        ///// </summary>
        //[ContextProperty("СпециальнаяПапка")]
        //public IValue SpecialFolder
        //{
        //    get
        //    {
        //        return new SpecialFolder();
        //    }
        //}

        [ContextMethod("ПолучитьПутьПапки")]
        public string GetFolderPath(IValue folder)
        {
            return System.Environment.GetFolderPath((System.Environment.SpecialFolder)folder.AsNumber());
        }

        /// <summary>
        /// Возвращает массив строк, содержащий имена логических дисков текущего компьютера.
        /// </summary>
        [ContextProperty("ИменаЛогическихДисков")]
        public ArrayImpl GetLogicalDrives
        {
            get
            {
                ArrayImpl arr = new ArrayImpl();
                var data = System.Environment.GetLogicalDrives();
                foreach (var itm in data)
                {
                    arr.Add(ValueFactory.Create(itm));
                }
                return arr;
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
        public IRuntimeContextInstance EnvironmentVariables()
        {
            SystemLogger.Write("WARNING! Deprecated method: 'SystemInfo.EnvironmentVariables' is deprecated, use 'EnvironmentVariables' from global context");
            var varsMap = new MapImpl();
            var allVars = System.Environment.GetEnvironmentVariables();
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
            SystemLogger.Write("WARNING! Deprecated method: 'SystemInfo.SetEnvironmentVariable' is deprecated, use 'SetEnvironmentVariable' from global context");
            System.Environment.SetEnvironmentVariable(varName, value);
        }

        /// <summary>
        /// Получить значение переменной среды.
        /// </summary>
        /// <param name="varName">Имя переменной</param>
        /// <returns>Строка. Значение переменной</returns>
        [ContextMethod("ПолучитьПеременнуюСреды", "GetEnvironmentVariable")]
        public IValue GetEnvironmentVariable(string varName)
        {
            SystemLogger.Write("WARNING! Deprecated method: 'SystemInfo.GetEnvironmentVariable' is deprecated, use 'GetEnvironmentVariable' from global context");
            string value = System.Environment.GetEnvironmentVariable(varName);
            if (value == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(value);

        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Create()
        {
            return new SystemEnvironmentContext();
        }
    }

    [SystemEnum("СпециальнаяПапка", "SpecialFolder")]
    class ServiceStartModeEnum : EnumerationContext
    {
        private ServiceStartModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public static ServiceStartModeEnum CreateInstance()
        {
            ServiceStartModeEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеРежимЗапуска", typeof(ServiceStartModeEnum));
            var enumValueType = TypeManager.RegisterType("РежимЗапуска", typeof(CLREnumValueWrapper<SpecialFolder>));

            instance = new ServiceStartModeEnum(type, enumValueType);

            instance.AddValue("РепозиторийДокументов", "Personal", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.Personal));
            instance.AddValue("ДанныеПриложений", "ApplicationData", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.ApplicationData));
            instance.AddValue("ЛокальныйКаталогДанныхПриложений", "LocalApplicationData", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.LocalApplicationData));
            instance.AddValue("КаталогРабочийСтол", "Desktop", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.Desktop));
            instance.AddValue("Вручную", "DesktopDirectory", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.DesktopDirectory));
            instance.AddValue("System", "MyMusic", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.MyMusic));
            instance.AddValue("System", "MyPictures", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.MyPictures));
            instance.AddValue("System", "Templates", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.Templates));
            instance.AddValue("System", "MyVideos", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.MyVideos));
            instance.AddValue("System", "CommonTemplates", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.CommonTemplates));
            instance.AddValue("System", "UserProfile", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.UserProfile));
            instance.AddValue("System", "CommonApplicationData", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.CommonApplicationData));

            return instance;
        }
    }

    public enum SpecialFolder
    {
        Personal = 0x05,
        ApplicationData = 0x1a,
        LocalApplicationData = 0x1c,
        Desktop = 0x00,
        DesktopDirectory = 0x10,
        MyMusic = 0x0d,
        MyPictures = 0x27,
        Templates = 0x15,
        MyVideos = 0x0e,
        CommonTemplates = 0x2d,
        Fonts = 0x14,
        UserProfile = 0x28,
        CommonApplicationData = 0x23
    }
}
