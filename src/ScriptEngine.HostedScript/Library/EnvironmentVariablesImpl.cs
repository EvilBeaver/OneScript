/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Работа с переменными окружения")]
    public class EnvironmentVariablesImpl : GlobalContextBase<EnvironmentVariablesImpl>
    {

        /// <summary>
        /// Возвращает соответствие переменных среды. Ключом является имя переменной, а значением - значение переменной
        /// </summary>
        /// <param name="target">Расположение переменной среды</param>
        /// <example>
        /// Для Каждого Переменная Из ПеременныеСреды() Цикл
        ///     Сообщить(Переменная.Ключ + " = " + Переменная.Значение);
        /// КонецЦикла;
        /// </example>
        /// <returns>Соответствие</returns>
        [ContextMethod("ПеременныеСреды", "EnvironmentVariables")]
        public MapImpl EnvironmentVariables(EnvironmentVariableTargetEnum target = EnvironmentVariableTargetEnum.Process)
        {
            EnvironmentVariableTarget targetParam = GetSystemEnvVariableTarget(target);
            var varsMap = new MapImpl();
            var allVars = System.Environment.GetEnvironmentVariables(targetParam);
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
        /// По умолчанию переменная устанавливается в области видимости процесса и очищается после его завершения.
        /// </summary>
        /// <param name="varName">Имя переменной</param>
        /// <param name="value">Значение переменной</param>
        /// <param name="target">Расположение переменной среды</param>
        [ContextMethod("УстановитьПеременнуюСреды", "SetEnvironmentVariable")]
        public void SetEnvironmentVariable(string varName, string value, EnvironmentVariableTargetEnum target = EnvironmentVariableTargetEnum.Process)
        {
            EnvironmentVariableTarget targetParam = GetSystemEnvVariableTarget(target);
            System.Environment.SetEnvironmentVariable(varName, value, targetParam);
        }

        /// <summary>
        /// Получить значение переменной среды.
        /// </summary>
        /// <param name="varName">Имя переменной</param>
        /// <param name="target">Расположение переменной среды</param>
        /// <returns>Строка. Значение переменной</returns>
        [ContextMethod("ПолучитьПеременнуюСреды", "GetEnvironmentVariable")]
        public IValue GetEnvironmentVariable(string varName, EnvironmentVariableTargetEnum target = EnvironmentVariableTargetEnum.Process)
        {
            EnvironmentVariableTarget targetParam = GetSystemEnvVariableTarget(target);
            string value = System.Environment.GetEnvironmentVariable(varName, targetParam);
            if (value == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(value);

        }

        public static IAttachableContext CreateInstance()
        {
            return new EnvironmentVariablesImpl();
        }

        private static EnvironmentVariableTarget GetSystemEnvVariableTarget(EnvironmentVariableTargetEnum target)
        {
            EnvironmentVariableTarget targetParam = EnvironmentVariableTarget.Process;
            switch (target)
            {
                case EnvironmentVariableTargetEnum.Process:
                    targetParam = EnvironmentVariableTarget.Process;
                    break;
                case EnvironmentVariableTargetEnum.User:
                    targetParam = EnvironmentVariableTarget.User;
                    break;
                case EnvironmentVariableTargetEnum.Machine:
                    targetParam = EnvironmentVariableTarget.Machine;
                    break;
            }
            return targetParam;
        }
    }
}
