/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace NUnitTests
{
	internal class EngineWrapperNUnit : IHostApplication
	{
		private string[] commandLineArgs;

		public HostedScriptEngine Engine { get; private set; }

		public HostedScriptEngine StartEngine()
		{
			Engine = new HostedScriptEngine();
			Engine.Initialize();

			commandLineArgs = new string[]
			{
			};

			return Engine;
		}
		
		public int RunTestString(string source)
		{
			var process = Engine.CreateProcess(this, Engine.Loader.FromString(source));
			return process.Start();
		}
		
		private int RunTestScript(ICodeSource source, string resourceName)
		{
			var module = Engine.GetCompilerService().CreateModule(source);

			Engine.LoadUserScript(new UserAddedScript
			{
				Type = UserAddedScriptType.Class,
				Module = module,
				Symbol = resourceName
			});

			var process = Engine.CreateProcess(this, source);
			return process.Start();
		}

		internal int RunTestScriptFromPath(string scriptFilePath, string argsScript = "")
		{
			if (argsScript != "")
				commandLineArgs = argsScript.Split(' ');

			var sourceToCompile = Engine.Loader.FromFile(scriptFilePath);

			return RunTestScript(sourceToCompile, scriptFilePath);
		}

		public string[] GetCommandLineArguments()
		{
			return commandLineArgs;
		}

		public bool InputString(out string result, int maxLen)
		{
			result = "";
			return false;
		}

		public void ShowExceptionInfo(Exception exc)
		{
			throw exc;
		}

		public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
			Console.WriteLine(str);
		}
	}
}