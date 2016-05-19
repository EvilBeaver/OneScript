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
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;

namespace oscript
{
    class ExecuteScriptBehavior : AppBehavior, IHostApplication, ISystemLogWriter
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
                Echo(String.Format("Script file is not found '{0}'", _path));
                return 2;
            }

            SystemLogger.SetWriter(this);

            var hostedScript = new HostedScriptEngine();
            hostedScript.CustomConfig = ScriptFileHelper.CustomConfigPath(_path);
            ScriptFileHelper.OnBeforeScriptRead(hostedScript);
            var source = hostedScript.Loader.FromFile(_path);

            Process process;
            try
            {
                process = hostedScript.CreateProcess(this, source);
            }
            catch(Exception e)
            {
                this.ShowExceptionInfo(e);
                return 1;
            }

            return process.Start();
        }

        #region IHostApplication Members

        public void Echo(string text, EchoStatus status = EchoStatus.Undefined)
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

        public void Write(string text)
        {
            Console.Error.WriteLine(text);
        }
    }
}
