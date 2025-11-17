/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// DTO для описания теста, возвращаемого методом ЗагрузитьТесты из testrunner.os
    /// </summary>
    public class BslTestDescriptionDto
    {
        public BslTestDescriptionDto(
            UserScriptContextInstance testObject,
            string className,
            string fullName,
            string methodName,
            string representation,
            IValue[] testParameters)
        {
            TestObject = testObject;
            ClassName = className;
            FullName = fullName;
            MethodName = methodName;
            Representation = representation;
            TestParameters = testParameters;
        }

        /// <summary>
        /// Тестовый объект (экземпляр класса теста)
        /// </summary>
        public UserScriptContextInstance TestObject { get; }
        
        /// <summary>
        /// Имя класса теста
        /// </summary>
        public string ClassName { get; }
        
        /// <summary>
        /// Полное имя тестового случая
        /// </summary>
        public string FullName { get; }
        
        /// <summary>
        /// Имя тестового метода
        /// </summary>
        public string MethodName { get; }
        
        /// <summary>
        /// Представление теста (для параметризованных тестов)
        /// </summary>
        public string Representation { get; }
        
        /// <summary>
        /// Параметры теста (для параметризованных тестов), может быть null
        /// </summary>
        public IValue[] TestParameters { get; }
    }
}

