/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;

using NUnit.Framework;

namespace oscript
{
	[TestFixture]
	public class oscriptTests
	{
		[SetUp]
		public void SetUp()
		{
			var path = AppDomain.CurrentDomain.BaseDirectory;
			_simpleScriptFullPath = Path.Combine(path, "simpleScript.os");
			_uncompilableScriptFullPath = Path.Combine(path, "uncompilableScript.os");
		}

		private string _simpleScriptFullPath;

		private string _uncompilableScriptFullPath;

		private static string GetLastOutput(string text)
		{
			var array = text.Split(new[]
			{
				"\r\n"
			}, StringSplitOptions.RemoveEmptyEntries);

			return array[array.Length - 1];
		}

		[Test]
		[Category("Compile")]
		public void BasicCompileBehaviourWorks()
		{
			var args = new[]
			{
				"-compile",
				_simpleScriptFullPath
			};

			Program.Main(args);
		}

		[Test]
		[Category("Make")]
		public void BasicMakeBehaviourNonsenseFileReturnsError()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var tempFolder = Path.GetTempPath();
				const string EXE_NAME = "test.exe";

				var exeFullPath = Path.Combine(tempFolder, EXE_NAME);

				if (File.Exists(exeFullPath))
					File.Delete(exeFullPath);

				var args = new[]
				{
					"-make",
					"nonsense.os",
					exeFullPath
				};

				Program.Main(args);

				var exeExists = File.Exists(exeFullPath);
				const string UNEXPECTED = "Make completed";
				var result = GetLastOutput(sw.ToString());

				Assert.AreNotEqual(UNEXPECTED, result);
				Assert.IsFalse(exeExists);
			}
		}

		[Test]
		[Category("Make")]
		public void BasicMakeBehaviourWorks()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var tempFolder = Path.GetTempPath();
				const string EXE_NAME = "test.exe";

				var exeFullPath = Path.Combine(tempFolder, EXE_NAME);

				if (File.Exists(exeFullPath))
					File.Delete(exeFullPath);

				var args = new[]
				{
					"-make",
					_simpleScriptFullPath,
					exeFullPath
				};

				Program.Main(args);

				var exeExists = File.Exists(exeFullPath);
				const string EXPECTED = "Make completed";
				var result = GetLastOutput(sw.ToString());

				Assert.AreEqual(EXPECTED, result);
				Assert.IsTrue(exeExists, "Exptected file: " + exeFullPath);
			}
		}

		[Test]
		[Category("Script execution")]
		public void FileNotFoundReturnsError()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var args = new[]
				{
					"nonsense.os"
				};

				Program.Main(args);
				const string EXPECTED = "Script file is not found \'nonsense.os\'";
				var result = GetLastOutput(sw.ToString());
				Assert.AreEqual(EXPECTED, result);
			}
		}

		[Test]
		[Category("NoArgs")]
		public void ShowUsageBehaviourExecutes()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var args = new string[]
				{
				};
				Program.Main(args);
				const string EXPECTED = "  Runs as CGI application under HTTP-server (Apache/Nginx/IIS/etc...)";
				var result = GetLastOutput(sw.ToString());
				Assert.AreEqual(EXPECTED, result);
			}
		}

		[Test]
		[Category("Check")]
		public void SimpleScriptChecksOut()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var args = new[]
				{
					"-check",
					_simpleScriptFullPath
				};

				Program.Main(args);
				var result = sw.ToString();
				var expected = $"No errors.{Environment.NewLine}";
				Assert.AreEqual(expected, result);
			}
		}

		[Test]
		[Category("Script execution")]
		public void SimpleScriptExecutes()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var args = new[]
				{
					_simpleScriptFullPath
				};

				Program.Main(args);
				var result = sw.ToString();
				Assert.IsEmpty(result);
			}
		}

		[Test]
		[Category("Check")]
		public void UncompilableScriptFailsCheck()
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var args = new[]
				{
					"-check",
					_uncompilableScriptFullPath
				};

				Program.Main(args);
				var result = sw.ToString();
				var unexpected = $"No errors.{Environment.NewLine}";
				Assert.AreNotEqual(unexpected, result);
			}
		}
	}
}
