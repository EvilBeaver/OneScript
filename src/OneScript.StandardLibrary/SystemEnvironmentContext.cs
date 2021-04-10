/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using OneScript.Commons;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine;
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
        public long TickCount
        {
            get
            {
                var unsig = (uint)System.Environment.TickCount;
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
            var typedValue = folder as ClrEnumValueWrapper<System.Environment.SpecialFolder>;
            if (typedValue == null)
                throw RuntimeException.InvalidArgumentType();

            return System.Environment.GetFolderPath(typedValue.UnderlyingValue);
            
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
                var data = System.Environment.GetLogicalDrives();
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
            SystemLogger.Write(string.Format(Locale.NStr("en='{0}';ru='{1}'"),
                "WARNING! Deprecated method: \"SystemInfo.SetEnvironmentVariable\" is deprecated, use \"SetEnvironmentVariable\" from global context",
                "Предупреждение! Устаревший метод: \"СистемнаяИнформация.УстановитьПеременнуюСреды\" устарел, используйте метод глобального контекста \"УстановитьПеременнуюСреды\""));
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
            SystemLogger.Write(string.Format(Locale.NStr("en='{0}';ru='{1}'"),
               "WARNING! Deprecated method: \"SystemInfo.GetEnvironmentVariable\" is deprecated, use \"GetEnvironmentVariable\" from global context",
                "Предупреждение! Устаревший метод: \"СистемнаяИнформация.ПолучитьПеременнуюСреды\" устарел, используйте метод глобального контекста \"ПолучитьПеременнуюСреды\""));
            string value = System.Environment.GetEnvironmentVariable(varName);
            if (value == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(value);

        }

        [ScriptConstructor]
        public static SystemEnvironmentContext Create()
        {
            return new SystemEnvironmentContext();
        }
    }
}
