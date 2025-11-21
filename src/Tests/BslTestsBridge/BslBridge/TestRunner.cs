/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OneScript.StandardLibrary.Collections.ValueTable;
using ScriptEngine.Machine;
using NUnit.Framework;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace BslTestsBridge.BslBridge
{
    public class TestRunner
    {
        private readonly BslExecutor _bslExecutor;
        
        public TestRunner()
        {
            var testrunnerLocation = Utils.GetTestRunnerPath();

            try
            {
                _bslExecutor = new BslExecutor(testrunnerLocation);
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException($"testrunner.os not found at {testrunnerLocation}", e);
            }
        }
        
        /// <summary>
        /// Обнаружение тестов, находящихся в указанной директории.
        /// Каждый тест это test fixture с точки зрения NUnit, т.е. содержит один или несколько тестовых методов.
        /// </summary>
        /// <param name="testsDir">Каталог с тестами</param>
        /// <returns></returns>
        public IEnumerable<BslTestsFileDto> GetTests(string testsDir)
        {
            var result = _bslExecutor.Execute("ОбнаружитьКлассыТестов", new[] { ValueFactory.Create(testsDir) });
            WriteBslMessages(result);
            
            var vt = result.MethodResult as ValueTable;
            if (vt == null) 
                throw new Exception("Не удалось получить список тестов");

            return vt.Select((ValueTableRow row) => new BslTestsFileDto(row.Get(0).ToString(), row.Get(1).ToString()))
                .ToList();
        }

        public UserScriptContextInstance LoadTestInstance(BslTestsFileDto test)
        {
            var loadInstanceResult = _bslExecutor.Execute("ЗагрузитьИнстансТеста", new[] { ValueFactory.Create(test.Path) });
            WriteBslMessages(loadInstanceResult);
            
            return (UserScriptContextInstance)loadInstanceResult.MethodResult;
        }

        public List<BslTestDescriptionDto> GetTestsDescriptions(BslTestsFileDto test)
        {
            // Загрузить тестовый объект
            var testInstance = LoadTestInstance(test);
            
            // Получить список тестов через testrunner.os метод ЗагрузитьТесты
            var loadTestsResult = _bslExecutor.Execute("ЗагрузитьТесты", new[] {
                testInstance, ValueFactory.Create(test.FixtureName)});

            WriteBslMessages(loadTestsResult);

            var testDescriptionsArray = loadTestsResult.MethodResult as ArrayImpl;
            if (testDescriptionsArray == null)
                throw new Exception($"Не удалось получить список тестов из файла {test.Path}");

            // Конвертируем массив структур в список DTO
            return ReadTestsDescriptions(testDescriptionsArray);
        }

        public LifecycleMethods GetLifecycleMethods(UserScriptContextInstance testInstance)
        {
            return ReadLifecycleMethods(testInstance);
        }
        
        private static List<BslTestDescriptionDto> ReadTestsDescriptions(ArrayImpl testDescriptionsArray)
        {
            var testDescriptions = new List<BslTestDescriptionDto>();
            foreach (var testDescriptionValue in testDescriptionsArray)
            {
                if (!(testDescriptionValue is StructureImpl testDescriptionStruct))
                    throw new Exception($"Ожидалась структура описания теста, получен {testDescriptionValue?.GetType().Name ?? "null"}");

                // Получаем свойства структуры через GetIndexedValue
                var testObjectValue = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ТестОбъект"));
                var testObject = testObjectValue as UserScriptContextInstance;
                if (testObject == null)
                    throw new Exception("Не удалось получить тестовый объект из структуры описания теста");

                var className = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ИмяКласса")).ToString()!;
                var fullName = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ПолноеИмя")).ToString()!;
                var methodName = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ИмяМетода")).ToString()!;
                var representation = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("Представление")).ToString()!;

                var parametersValue = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ПараметрыТеста"));
                IValue[] testParameters = null;
                // Проверяем, что параметры не являются Неопределено
                if (parametersValue != null &&
                    parametersValue.SystemType != BasicTypes.Undefined &&
                    parametersValue is ArrayImpl parametersArray)
                {
                    testParameters = parametersArray.ToArray();
                }
                else
                {
                    testParameters = Array.Empty<IValue>();
                }

                var dto = new BslTestDescriptionDto(
                    testObject,
                    className,
                    fullName,
                    methodName,
                    representation,
                    testParameters);

                testDescriptions.Add(dto);
            }

            return testDescriptions;
        }
        
        private LifecycleMethods ReadLifecycleMethods(UserScriptContextInstance testInstance)
        {
            var lifecycleResult = _bslExecutor.Execute("ПолучитьМетодыЖизненногоЦикла", new[] { testInstance });
            WriteBslMessages(lifecycleResult);
            var lifecycleMethodsStructure = lifecycleResult.MethodResult as StructureImpl;
            if (lifecycleMethodsStructure == null)
                throw new Exception("Не удалось получить методы жизненного цикла из testrunner.os");

            // Получаем методы жизненного цикла
            var lifecycleMethodsNames = ParseLifecycleMethods(lifecycleMethodsStructure);
            return lifecycleMethodsNames;
        }
        
        private static LifecycleMethods ParseLifecycleMethods(StructureImpl lifecycleStructure)
        {
            var lifecycleMethods = new LifecycleMethods();

            var beforeAllValue = lifecycleStructure.GetIndexedValue(ValueFactory.Create("BeforeAll"));
            lifecycleMethods.BeforeAll = ExtractLifecycleMethodNames(beforeAllValue);

            var beforeEachValue = lifecycleStructure.GetIndexedValue(ValueFactory.Create("BeforeEach"));
            lifecycleMethods.BeforeEach = ExtractLifecycleMethodNames(beforeEachValue);

            var afterEachValue = lifecycleStructure.GetIndexedValue(ValueFactory.Create("AfterEach"));
            lifecycleMethods.AfterEach = ExtractLifecycleMethodNames(afterEachValue);

            var afterAllValue = lifecycleStructure.GetIndexedValue(ValueFactory.Create("AfterAll"));
            lifecycleMethods.AfterAll = ExtractLifecycleMethodNames(afterAllValue);

            return lifecycleMethods;
        }

        private static string[] ExtractLifecycleMethodNames(IValue lifecycleValue)
        {
            if (lifecycleValue.SystemType == BasicTypes.Undefined)
                return Array.Empty<string>();

            if (lifecycleValue is ArrayImpl lifecycleArray)
            {
                var methods = new List<string>();
                foreach (var value in lifecycleArray)
                {
                    var methodName = value?.ToString();
                    if (!string.IsNullOrWhiteSpace(methodName))
                    {
                        methods.Add(methodName);
                    }
                }

                return methods.ToArray();
            }

            throw new Exception("Ожидался массив методов жизненного цикла от testrunner.os");
        }
        
        private void WriteBslMessages(BslProcessResult result)
        {
            result.FlushIntoWriter(TestContext.Out, TestContext.Error);
        }
    }
}