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
using System.Text;

using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace oscript
{
	internal class ExecuteScriptBehavior : AppBehavior, IHostApplication, ISystemLogWriter
	{
		private readonly string _path;

		private readonly string[] _scriptArgs;

		public ExecuteScriptBehavior(string path, string[] args)
		{
			_scriptArgs = args;
			_path = path;
		}

		public override int Execute()
		{
			if (!File.Exists(_path))
			{
				Echo($"Script file is not found '{_path}'");
				return 2;
			}

			SystemLogger.SetWriter(this);

			var hostedScript = new HostedScriptEngine
			{
				CustomConfig = ScriptFileHelper.CustomConfigPath(_path)
			};
			ScriptFileHelper.OnBeforeScriptRead(hostedScript);
			var source = hostedScript.Loader.FromFile(_path);

			Process process;
			try
			{
				process = hostedScript.CreateProcess(this, source);
			}
			catch (Exception e)
			{
				ShowExceptionInfo(e);
				return 1;
			}

			var result = process.Start();
			hostedScript.Finalize();

			ScriptFileHelper.OnAfterScriptExecute(hostedScript);

			return result;
		}

		public void Write(string text)
		{
			Console.Error.WriteLine(text);
		}

		#region IHostApplication Members

		public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
			ConsoleHostImpl.Echo(text, status);
		}

		public void ShowExceptionInfo(Exception exc)
		{
			ConsoleHostImpl.ShowExceptionInfo(exc);
		}

		public bool InputString(out string result, int maxLen)
		{
			return ConsoleHostImpl.InputString(out result, maxLen);
		}

		public string[] GetCommandLineArguments()
		{
			return _scriptArgs;
		}

		#endregion
	}
}