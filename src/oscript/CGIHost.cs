using ScriptEngine.HostedScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class CGIHost : IHostApplication
    {
        public Encoding Encoding { get; set; }

        public CGIHost()
        {
            Encoding = new UTF8Encoding();
        }

        public void Echo(string str)
        {
            using(var stream = Console.OpenStandardOutput())
            {
                var bytes = Encoding.GetBytes(str);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void ShowExceptionInfo(Exception exc)
        {
            Echo(exc.ToString());
        }

        public bool InputString(out string result, int maxLen)
        {
            result = null;
            return false;
        }

        public string[] GetCommandLineArguments()
        {
            return new string[0];
        }
    }
}
