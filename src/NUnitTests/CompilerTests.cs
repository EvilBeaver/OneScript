/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using NUnit.Framework;
using ScriptEngine.Compiler;

namespace NUnitTests
{
	[TestFixture]
	public class CompilerTests
	{
		private EngineWrapperNUnit host;

		[OneTimeSetUp]
		public void Init()
		{
			host = new EngineWrapperNUnit();
			host.StartEngine();
			var solutionRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");
			host.Engine.InitExternalLibraries(Path.Combine(solutionRoot, "oscript-library", "src"), null);
		}

		[Test]
		public void TestNoSemicolonBeforeEndProcedure()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Возврат
				КонецПроцедуры");

			var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeEndFunction()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Функция Фун1()
					Возврат 4
				КонецФункции");

			var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeEndDo()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Для Инд = 1 По 10 Цикл
					Прервать
				КонецЕсли");

			var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeEndIf()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда
					Ф = 1
				ИначеЕсли Истина Тогда
					Ф = 2
				Иначе
					Ф = 3
				КонецЕсли");

			var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
		}
		

		[Test]
		public void TestEndFunctionDoesNotEndIf()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда
					Ф = 1
				КонецФункции");

			bool exceptionThrown = false;
			try
			{
				var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецФункции закрыл Если!!!");
		}

		[Test]
		public void TestEndDoDoesNotEndIf()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда
					Ф = 1
				КонецЦикла");

			bool exceptionThrown = false;
			try
			{
				var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецЦикла закрыл Если!!!");
		}

		[Test]
		public void TestEndIfDoesNotEndDo()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Пока Истина Цикл
					Ф = 1
				КонецЕсли");

			bool exceptionThrown = false;
			try
			{
				var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецЕсли закрыл Пока!!!");
		}

		[Test(Description = "Компилируется вызов метода с пропуском параметров")]
		public void TestCompileMethodCallWithoutAllParams()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Функция Ф3(П1, П2, П3)
					Возврат """" + П1 + П2 + П3
				КонецФункции
				Функция Ф2(П1, П2, П3 = Неопределено)
					Возврат """" + П1 + П2 + П3
				КонецФункции
				Функция Ф1(П1, П2 = Неопределено, П3 = Неопределено)
					Возврат """" + П1 + П2 + П3
				КонецФункции
				Р = Ф3(,,);
				Р = Ф2(,) + Ф2(,,);
				Р = Ф1(,,) + Ф1(,);
				");

			var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
		}
		
		[Test(Description = "Не компилируется вызов метода вообще без параметров")]
		public void TestCantCompileCallWithoutParams()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Функция Ф1(П1)
					Возврат П1;
				КонецФункции
				Р = Ф1();
				");

			bool exceptionThrown = false;
			try
			{
				var module = host.Engine.GetCompilerService().CreateModule(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Не должно было скомпилироваться!");
		}
		
		
	}
}