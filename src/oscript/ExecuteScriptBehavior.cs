using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;

namespace oscript
{
    class ExecuteScriptBehavior : AppBehavior, IHostApplication
    {
        string[] _scriptArgs;
        string _path;

        public ExecuteScriptBehavior(string path, string[] args)
        {
            _scriptArgs = args;
            _path = path;
        }
        
        public override int Execute()
        {
            if (!System.IO.File.Exists(_path))
            {
                throw new System.IO.FileNotFoundException("Script file is not found", _path);
            }

            var hostedScript = new HostedScriptEngine();
            var source = hostedScript.Loader.FromFile(_path);
            var process = hostedScript.CreateProcess(this, source);

            return process.Start();
        }

        #region IHostApplication Members

        public void Echo(string text)
        {
            Console.WriteLine(text);
        }

        public void ShowExceptionInfo(Exception exc)
        {
            Console.WriteLine(exc.ToString());
        }

        public bool InputString(out string result, int maxLen)
        {
            if (maxLen == 0)
            {
                result = Console.ReadLine();
            }
            else
            {
                result = Console.ReadLine().Substring(0, maxLen);
            }

            return result.Length > 0;

        }

        public string[] GetCommandLineArguments()
        {
            return _scriptArgs;
        }

        #endregion
    }
}
