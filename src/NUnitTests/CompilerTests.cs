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

		#region Отсутствие точки с запятой перед завершением блока

		[Test]
		public void TestNoSemicolonBeforeEndProcedure()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Возврат
				КонецПроцедуры");

		    _ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeEndFunction()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Функция Фун1()
					Возврат 4
				КонецФункции");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeEndDo()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Для Инд = 1 По 10 Цикл
					Прервать
				КонецЦикла");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeElseOrEndIf()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда
					Ф = 1
				ИначеЕсли Истина Тогда
					Ф = 2
				Иначе
					Ф = 3
				КонецЕсли");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestNoSemicolonBeforeExceptionOrEndTry()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Попытка
					Ф = 1
				Исключение
					ВызватьИсключение
				КонецПопытки");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}


		[Test]
		public void TestNoSemicolonBeforeEndOfText()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Возврат
				КонецПроцедуры
				Ф = 0");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}
		#endregion

		#region Точка с запятой перед завершением блока

		[Test]
		public void TestSemicolonBeforeEndProcedure()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Возврат;
				КонецПроцедуры");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestSemicolonBeforeEndFunction()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Функция Фун1()
					Возврат 4;
				КонецФункции");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestSemicolonBeforeEndDo()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Для Инд = 1 По 10 Цикл
					Прервать;
				КонецЦикла");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestSemicolonBeforeElseOrEndIf()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда
					Ф = 1;
				ИначеЕсли Истина Тогда
					Ф = 2;
				Иначе
					Ф = 3;
				КонецЕсли");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}

		[Test]
		public void TestSemicolonBeforeExceptionOrEndTry()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Попытка
					Ф = 1;
				Исключение
					ВызватьИсключение;
				КонецПопытки");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}


		[Test]
		public void TestSemicolonBeforeEndOfText()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Возврат
				КонецПроцедуры
				Ф = 0;");

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
		}
		#endregion


		#region Согласованное завершение структурных операторов
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
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
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
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
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
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецЕсли закрыл Пока!!!");
		}
		[Test]
		public void TestEndFunctionDoesNotEndProcedure()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц()
					Возврат
				КонецФункции");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецФункции закрыл Процедуру!!!");
		}

		[Test]
		public void TestEndProcedureDoesNotEndFunction()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Функция Функ()
					Возврат 0
				КонецПроцедуры");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецПроцедуры закрыл Функцию!!!");
		}

		[Test]
		public void TestElseifDoesNotEndProcedure()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц()
					Возврат
				ИначеЕсли");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "ИначеЕсли закрыл Процедуру!!!");
		}

		[Test]
		public void TestEndTryDoesNotEndProcedure()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц()
					Возврат
				КонецПопытки");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "КонецПопытки закрыл Процедуру!!!");
		}
		#endregion

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

			_ = host.Engine.GetCompilerService().Compile(moduleSource);
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
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Не должно было скомпилироваться!");
		}

		[Test]
		public void TestReturnBeforeException()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц()
					Попытка
						Возврат
					Исключение
					КонецПопытки
				КонецПроцедуры");
            _ = host.Engine.GetCompilerService().Compile(moduleSource);
        }


		[Test]
		public void TestElseifDoesNotDelimitAddHandler()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда 
					ДобавитьОбработчик ЭтотОбъект.Событие ИначеЕсли ЭтотОбъект.Обработчик
				КонецЕсли");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "ИначеЕсли разделяет параметры ДобавитьОбработчик!!!");
		}


		[Test]
		public void TestSemicolonBeforeProcedures()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Перем Глоб
				Процедура Проц1()
					Возврат
				КонецПроцедуры");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Нет точки с запятой между переменными модуля и процедурами!");
		}

		[Test]
		public void TestSemicolonAfterLocals()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Перем Локал
					Если Истина Тогда
						Локал = 1
					КонецЕсли
				КонецПроцедуры");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Нет точки с запятой между локалиными переменными и операторами!");
		}

		[Test]
		public void TestNoSemicolonBetweenProcedures()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Процедура Проц1()
					Возврат
				КонецПроцедуры;
				Процедура Проц2()
					Возврат
				КонецПроцедуры");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Точка с запятой между процедурами!");
		}

		[Test]
		public void TestSemicolonBetweenStatements1()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"
				Ф = 1
				Если Ложь Тогда 
					Ф = 2
				КонецЕсли");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Отсутствует точка с запятой между операторами!");
		}

		[Test]
		public void TestSemicolonBetweenStatements2()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"
				Если Ложь Тогда 
					Ф = 2
				КонецЕсли
				Ф = 1");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Отсутствует точка с запятой между операторами!");
		}

		[Test]
		public void TestSemicolonBetweenStatements3()
		{
			var moduleSource = host.Engine.Loader.FromString(
				@"Если Истина Тогда 
					Ф = 1
				КонецЕсли
				Если Ложь Тогда 
					Ф = 2
				КонецЕсли");

			bool exceptionThrown = false;
			try
			{
				_ = host.Engine.GetCompilerService().Compile(moduleSource);
			}
			catch (CompilerException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Отсутствует точка с запятой между операторами!");
		}
	}
}