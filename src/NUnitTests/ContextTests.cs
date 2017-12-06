/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ScriptEngine;
using ScriptEngine.Machine;

namespace NUnitTests
{
	[TestFixture]
	public class ContextTests : ISystemLogWriter
	{
		private EngineWrapperNUnit host;
		private readonly List<string> _messages = new List<string>();

		[OneTimeSetUp]
		public void Init()
		{
			host = new EngineWrapperNUnit();
			host.StartEngine();
			var solutionRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");
			host.Engine.InitExternalLibraries(Path.Combine(solutionRoot, "oscript-library", "src"), null);
			host.Engine.AttachAssembly(typeof(TestContextClass).Assembly);
		}

		[Test]
		public void TestICallDeprecatedAndHaveWarning()
		{
			SystemLogger.SetWriter(this);
			_messages.Clear();
			host.RunTestString(
				@"К = Новый ТестовыйКласс;
				К.УстаревшийМетод();
				К.ObsoleteMethod();
				К.УстаревшийМетод();");
			
			Assert.AreEqual(1, _messages.Count, "Только ОДНО предупреждение");
			Assert.IsTrue(_messages[0].IndexOf("УстаревшийМетод", StringComparison.InvariantCultureIgnoreCase) >= 0
			              || _messages[0].IndexOf("ObsoleteMethod", StringComparison.InvariantCultureIgnoreCase) >= 0);
		}

		[Test]
		public void TestICallGoodAndHaveNoWarning()
		{
			SystemLogger.SetWriter(this);
			_messages.Clear();
			host.RunTestString(
				@"К = Новый ТестовыйКласс;
				К.ХорошийМетод();
				К.GoodMethod();");
			
			Assert.AreEqual(0, _messages.Count, "Нет предупреждений");
		}

		[Test]
		public void TestICallDeprecatedAliasAndHaveWarning()
		{
			SystemLogger.SetWriter(this);
			_messages.Clear();
			host.RunTestString(
				@"К = Новый ТестовыйКласс;
				К.ObsoleteAlias();");
			
			Assert.AreEqual(1, _messages.Count, "Только ОДНО предупреждение");
			Assert.IsTrue(_messages[0].IndexOf("ObsoleteAlias", StringComparison.InvariantCultureIgnoreCase) >= 0);
		}

		[Test]
		public void TestICallDeprecatedAliasAndHaveException()
		{
			SystemLogger.SetWriter(this);
			_messages.Clear();
			var exceptionThrown = false;

			try
			{
				host.RunTestString(
					@"К = Новый ТестовыйКласс;
					К.VeryObsoleteAlias();");
			}
			catch (RuntimeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Безнадёжно устаревший метод должен вызвать исключение");
		}

		public void Write(string text)
		{
			_messages.Add(text);
		}

	}
}