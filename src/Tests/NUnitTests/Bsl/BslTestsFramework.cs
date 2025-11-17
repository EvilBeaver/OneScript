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
using System.Reflection;
using NUnit.Framework.Internal;
using NUnit.Framework.Interfaces;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Утилита для обнаружения и запуска тестов BSL. Оборачивает все сложности работы с вызовом bsl-раннера
    /// и предоставляет удобный интерфейс для получения списка тестов и тестовых методов.
    /// </summary>
    public class BslTestsFramework
    {
        private readonly BslScriptProcess _bslExecutor;

        private const string RunnerPath = "testrunner/testrunner.os";

        /// <summary>
        /// Использует раннер по умолчанию.
        /// </summary>
        public BslTestsFramework() : this(RunnerPath)
        {
        }
        
        /// <summary>
        /// Использует указанный bsl-раннер, взаимодействующий с тестами bsl.
        /// </summary>
        /// <param name="runnerPath">Путь к testrunner.os</param>
        public BslTestsFramework(string runnerPath)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var runnerFullPath = Path.Combine(dir, RunnerPath);
            
            _bslExecutor = new BslScriptProcess(runnerFullPath);
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
            if (vt == null) throw new Exception("Не удалось получить список тестов");

            return vt.Select((ValueTableRow row) => new BslTestsFileDto(row.Get(0).ToString(), row.Get(1).ToString()))
                .ToList();
        }

        /// <summary>
        /// Загружает тестовый набор из найденного файла.
        /// Если в процессе загрузки возникают ошибки, они пробрасываются на уровень выше, чтобы их обработал NUnit.
        /// </summary>
        public TestFixture LoadTestFixture(BslTestsFileDto discoveredFile)
        {
            var loadInstanceResult = _bslExecutor.Execute("ЗагрузитьИнстансТеста", new[] { ValueFactory.Create(discoveredFile.Path) });
            WriteBslMessages(loadInstanceResult);

            var testInstance = loadInstanceResult.MethodResult as UserScriptContextInstance;
            if (testInstance == null)
                throw new Exception($"Не удалось загрузить тестовый объект из файла {discoveredFile.Path}");

            // Получить список тестов через testrunner.os метод ЗагрузитьТесты
            var loadTestsResult = _bslExecutor.Execute("ЗагрузитьТесты", new[] {
                 testInstance, ValueFactory.Create(discoveredFile.FixtureName)});

            WriteBslMessages(loadTestsResult);

            var testDescriptionsArray = loadTestsResult.MethodResult as ArrayImpl;
            if (testDescriptionsArray == null)
                throw new Exception($"Не удалось получить список тестов из файла {discoveredFile.Path}");

            // Конвертируем массив структур в список DTO
            var testDescriptions = ReadTestsDescriptions(testDescriptionsArray);

            // Получить методы жизненного цикла через testrunner.os метод ПолучитьМетодыЖизненногоЦикла
            var lifecycleMethodsNames = ReadLifecycleMethods(testInstance);

            var fixtureType = CreateFixtureType(discoveredFile.FixtureName);
            var fixtureTypeInfo = new TypeWrapper(fixtureType);
            var testFixture = new BslTestFixture(fixtureTypeInfo)
            {
                Name = discoveredFile.FixtureName,
                FullName = discoveredFile.FixtureName,
                Fixture = testInstance
            };

            var methodsCache = new Dictionary<string, IMethodInfo>(StringComparer.OrdinalIgnoreCase);

            var beforeAllMethods = ResolveLifecycleMethodInfos(testInstance, lifecycleMethodsNames.BeforeAll, methodsCache);
            var beforeEachMethods = ResolveLifecycleMethodInfos(testInstance, lifecycleMethodsNames.BeforeEach, methodsCache);
            var afterEachMethods = ResolveLifecycleMethodInfos(testInstance, lifecycleMethodsNames.AfterEach, methodsCache);
            var afterAllMethods = ResolveLifecycleMethodInfos(testInstance, lifecycleMethodsNames.AfterAll, methodsCache);

            testFixture.ApplyLifecycleMethods(beforeAllMethods, beforeEachMethods, afterEachMethods, afterAllMethods);

            foreach (var testDescription in testDescriptions)
            {
                try
                {
                    var methodInfo = ResolveBslMethodInfo(testInstance, testDescription.MethodName, methodsCache);
                    var displayName = string.IsNullOrEmpty(testDescription.Representation)
                        ? testDescription.MethodName
                        : testDescription.Representation;
                    var fullName = string.IsNullOrEmpty(testDescription.FullName)
                        ? $"{testDescription.ClassName}.{displayName}"
                        : testDescription.FullName;

                    testFixture.AddBslTest(methodInfo, displayName, fullName, testDescription);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return testFixture;
        }

        private LifecycleMethods ReadLifecycleMethods(UserScriptContextInstance testInstance)
        {
            var lifecycleResult = _bslExecutor.Execute("ПолучитьМетодыЖизненногоЦикла", new[] { testInstance });
            WriteBslMessages(lifecycleResult);
            var lifecycleMethodsStructure = lifecycleResult.MethodResult as StructureImpl;
            if (lifecycleMethodsStructure == null)
                throw new Exception("Не удалось получить методы жизненного цикла из testrunner.os");

            // Получаем методы жизненного цикла
            var lifecycleMethodsNames = ReadLifecycleMethods(lifecycleMethodsStructure);
            return lifecycleMethodsNames;
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

                var className = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ИмяКласса")).ToString();
                var fullName = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ПолноеИмя")).ToString();
                var methodName = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ИмяМетода")).ToString();
                var representation = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("Представление")).ToString();

                var parametersValue = testDescriptionStruct.GetIndexedValue(ValueFactory.Create("ПараметрыТеста"));
                IValue[] testParameters = null;
                // Проверяем, что параметры не являются Неопределено
                if (parametersValue != null &&
                    parametersValue.SystemType != BasicTypes.Undefined &&
                    parametersValue is ArrayImpl parametersArray)
                {
                    testParameters = parametersArray.ToArray();
                }
                // Если параметры = Неопределено, то testParameters остается null

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

        private static LifecycleMethods ReadLifecycleMethods(StructureImpl lifecycleStructure)
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
            if (lifecycleValue == null || lifecycleValue.SystemType == BasicTypes.Undefined)
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

        private static void WriteBslMessages(BslProcessResult result)
        {
            foreach (var message in result.Messages)
            {
                Console.WriteLine(message.Text);
            }
        }

        private static Type CreateFixtureType(string fixtureName)
        {
            var builder = new ClassBuilder(typeof(UserScriptContextInstance));
            return builder
                .SetTypeName(fixtureName)
                .Build();
        }

        private IMethodInfo ResolveBslMethodInfo(
            UserScriptContextInstance testInstance,
            string methodName,
            IDictionary<string, IMethodInfo> cache)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("Имя метода теста не задано", nameof(methodName));

            if (cache.TryGetValue(methodName, out var cached))
                return cached;

            int methodNumber;
            try
            {
                methodNumber = testInstance.GetMethodNumber(methodName);
            }
            catch (RuntimeException ex)
            {
                throw new Exception($"Не удалось найти тестовый метод \"{methodName}\" в объекте теста.", ex);
            }

            var methodInfo = testInstance.GetMethodInfo(methodNumber) as BslScriptMethodInfo;
            
            var invokableMethod = new InvokableBslMethodInfo(methodInfo, _bslExecutor);
            var nunitMethod = new BslNUnitMethodInfo(invokableMethod, testInstance);

            cache[methodName] = nunitMethod;
            return nunitMethod;
        }

        private IMethodInfo ResolveLifecycleMethodInfo(
            UserScriptContextInstance testInstance,
            string methodName,
            IDictionary<string, IMethodInfo> cache)
        {
            if (cache.TryGetValue(methodName, out var cached))
                return cached;

            int methodNumber;
            try
            {
                methodNumber = testInstance.GetMethodNumber(methodName);
            }
            catch (RuntimeException)
            {
                return null;
            }
            
            var methodInfo = testInstance.GetMethodInfo(methodNumber) as BslScriptMethodInfo;
            if (methodInfo == null)
                throw new Exception($"Метод \"{methodName}\" не является скриптовым методом BSL.");

            var invokableMethod = new InvokableBslMethodInfo(methodInfo, _bslExecutor);
            var nunitMethod = new BslNUnitMethodInfo(invokableMethod, testInstance);

            cache[methodName] = nunitMethod;
            return nunitMethod;
        }

        private IMethodInfo[] ResolveLifecycleMethodInfos(
            UserScriptContextInstance testInstance,
            string[] methodNames,
            IDictionary<string, IMethodInfo> cache)
        {
            if (methodNames.Length == 0)
            {
                return Array.Empty<IMethodInfo>();
            }

            var result = new List<IMethodInfo>(methodNames.Length);
            foreach (var methodName in methodNames)
            {
                if (string.IsNullOrWhiteSpace(methodName))
                    continue;

                var resolvedMethod = ResolveLifecycleMethodInfo(testInstance, methodName, cache);
                if (resolvedMethod != null)
                {
                    result.Add(resolvedMethod);
                }
            }

            return result.ToArray();
        }

        public void ClearMessages()
        {
            _bslExecutor.ClearMessages();
        }
    }
}