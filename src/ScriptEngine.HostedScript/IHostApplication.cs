using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    public interface IHostApplication
    {
        void Echo(string str);
        void ShowExceptionInfo(Exception exc);
        public bool InputString(out string result, int maxLen);
        public string[] GetCommandLineArguments();
    }
}
