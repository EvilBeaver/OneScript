/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using OneScript.StandardLibrary;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace oscript
{
    internal class ExecuteCodeBehavior(string code, string[] args) : AppBehavior, IHostApplication, ISystemLogWriter
    {
        private readonly string _code = code;
        private readonly string[] _scriptArgs = args;

        public static AppBehavior Create(CmdLineHelper helper)
        {
            var code = helper.Next();
            if (string.IsNullOrEmpty(code))
                return null;

            return new ExecuteCodeBehavior(code, helper.Tail());
        }

        public override int Execute()
        {
            SystemLogger.SetWriter(this);

            var configPath = Path.Combine(Environment.CurrentDirectory, CfgFileConfigProvider.CONFIG_FILE_NAME);
            var builder = ConsoleHostBuilder.Create(configPath);
            var hostedScript = ConsoleHostBuilder.Build(builder);
            var source = hostedScript.Loader.FromString(_code);

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
            hostedScript.Dispose();
            return result;
        }

        #region IHostApplication Members

        public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
            => ConsoleHostImpl.Echo(text, status);

        public void ShowExceptionInfo(Exception exc)
            => ConsoleHostImpl.ShowExceptionInfo(exc);

        public bool InputString(out string result, string prompt, int maxLen, bool multiline)
            => ConsoleHostImpl.InputString(out result, prompt, maxLen, multiline);

        public string[] GetCommandLineArguments()
            => _scriptArgs;

        #endregion

        public void Write(string text)
            => Console.Error.WriteLine(text);
    }
}
