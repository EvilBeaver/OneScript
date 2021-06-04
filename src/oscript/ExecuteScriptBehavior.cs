/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.StandardLibrary;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
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
        
        public IDebugController DebugController { get; set; }

        public override int Execute()
        {
            if (!System.IO.File.Exists(_path))
            {
                Echo($"Script file is not found '{_path}'");
                return 2;
            }

            SystemLogger.SetWriter(this);

            var builder = ConsoleHostBuilder.Create(_path);
            builder.WithDebugger(DebugController);

            var hostedScript = ConsoleHostBuilder.Build(builder);

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

            var result = process.Start();
            hostedScript.Dispose();

            ScriptFileHelper.OnAfterScriptExecute(hostedScript);

            return result;
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

        public bool InputString(out string result, string prompt, int maxLen, bool multiline)
        {
            return ConsoleHostImpl.InputString(out result, prompt, maxLen, multiline);
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
