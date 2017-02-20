using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace oscript
{
    internal class SimpleConsoleHost : IHostApplication
    {

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
            return null;
        }
    }
}
