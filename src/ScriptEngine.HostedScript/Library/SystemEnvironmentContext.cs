using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
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
}
