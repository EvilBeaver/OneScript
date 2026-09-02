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
using ScriptEngine.Machine.Debugger;

namespace oscript
{
    class ExecuteScriptBehavior : AppBehavior, IHostApplication, ISystemLogWriter
    {
        protected string[] _scriptArgs;
        protected string _path;

        public ExecuteScriptBehavior(string path, string[] args)
        {
            _scriptArgs = args;
            _path = path;
        }
        
        public IDebugger DebugController { get; set; } = new DisabledDebugger();
        
        public string CodeStatFile { get; set; }

        public bool CodeStatisticsEnabled { get; set; }

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
            CodeStatHub codeStatHub = null;
            CodeStatProcessor cliSession = null;
            if (CodeStatisticsEnabled)
            {
                codeStatHub = new CodeStatHub();
                builder.Services.RegisterSingleton(codeStatHub);
                builder.Services.RegisterSingleton<ICodeStatCollector>(codeStatHub);
                if (CodeStatFile != null)
                    cliSession = codeStatHub.StartSession();
            }

            var hostedScript = ConsoleHostBuilder.Build(builder);
            
            var source = hostedScript.Loader.FromFile(_path);
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

            if (cliSession != null && codeStatHub != null)
            {
                codeStatHub.FinishSession(cliSession);
                var statsWriter = new CodeStatWriter(CodeStatFile, CodeStatWriterType.JSON);
                statsWriter.Write(cliSession.GetStatData());
            }

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
