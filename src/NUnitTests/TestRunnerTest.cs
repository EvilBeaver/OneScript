/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;

using NUnit.Framework;
using ScriptEngine.Machine;

namespace NUnitTests
{
	[TestFixture]
	public class UnitTestWrapper
	{
		private const int TEST_STATE_FAILED = 3;

		private EngineWrapperNUnit host;

		private string solutionRoot;

		[OneTimeSetUp]
		public void Initialize()
		{
			host = new EngineWrapperNUnit();
			host.StartEngine();
			solutionRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");
			host.Engine.InitExternalLibraries(Path.Combine(solutionRoot, "oscript-library", "src"), null);
		}

		private void RunSpecificTest(string testName)
		{
			var testRunnerPath = Path.Combine(solutionRoot, "tests", "testrunner.os");

			Assert.IsTrue(File.Exists(testRunnerPath),
				"Запускатель тестов отсутствует по пути " + testRunnerPath);

			var specificTestPath = Path.Combine(solutionRoot, "tests", testName);
			var result = host.RunTestScriptFromPath(testRunnerPath, $"-run {specificTestPath}");

			if (result == TEST_STATE_FAILED)
			{
				Assert.Fail("Есть непройденные тесты!");
			}
		}

		[Test]
		public void Test_Reflector()
		{
			RunSpecificTest(@"reflector.os");
        }
        [Test]
        public void Test_Zip()
        {
            RunSpecificTest(@"zip.os");
        }
		
		[Test]
		public void Test_XmlWrite()
		{
			RunSpecificTest(@"xmlwrite.os");
		}

		[Test]
		[Ignore("Внутри валится очень много тестов, надо чинить механизм.")]
		public void RunEngineTests()
		{
			var testRunnerPath = Path.Combine(solutionRoot, "tests", "testrunner.os");

			Assert.IsTrue(File.Exists(testRunnerPath),
						"Запускатель тестов отсутствует по пути " + testRunnerPath);

			var result = host.RunTestScriptFromPath(testRunnerPath, "-runall " + new FileInfo(testRunnerPath).Directory.FullName);

			if (result == TEST_STATE_FAILED)
				Assert.Fail("Есть непройденные тесты!");
		}

		[Test]
		public void CheckIKnowThisObject()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Перем А Экспорт;
				Функция ФЭтотОбъект(Знач Который = 0) Экспорт
					Возврат ?(Который = 0, ЭтотОбъект, ThisObject);
				КонецФункции
				A = 333;");
			var module = host.Engine.GetCompilerService().Compile(moduleSource);
			var loadedModule = host.Engine.EngineInstance.LoadModuleImage(module);
			var instance = host.Engine.EngineInstance.NewObject(loadedModule);
			var methodIndex = instance.FindMethod("ФЭтотОбъект");

			IValue asRus, asEng;

			instance.CallAsFunction(methodIndex, new[]{ValueFactory.Create(0)}, out asRus);
			instance.CallAsFunction(methodIndex, new[]{ValueFactory.Create(1)}, out asEng);
			
			Assert.AreEqual(asRus, asEng); // Тот же самый объект
		}
	}
}