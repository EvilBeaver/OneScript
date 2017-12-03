/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using NUnit.Framework;

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
	}
}