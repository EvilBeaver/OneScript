/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace oscript
{
	internal class DoNothingHost : IHostApplication
	{
		public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
		}

		public void ShowExceptionInfo(Exception exc)
		{
		}

		public bool InputString(out string result, string prompt, int maxLen, bool multiline)
		{
			result = "";
			return true;
		}

		public string[] GetCommandLineArguments()
		{
			return new[]
			{
				""
			};
		}
	}
}