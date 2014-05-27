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
        bool InputString(out string result, int maxLen);
        string[] GetCommandLineArguments();
    }
}
