using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace oscript
{
    class DoNothingHost : IHostApplication
    {
        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            
        }

        public void ShowExceptionInfo(Exception exc)
        {
            
        }

        public bool InputString(out string result, int maxLen)
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
