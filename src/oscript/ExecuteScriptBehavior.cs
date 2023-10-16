﻿/*----------------------------------------------------------
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
        protected string[] _scriptArgs;
        protected string _path;

        public ExecuteScriptBehavior(string path, string[] args)
        {
            _scriptArgs = args;
            _path = path;
        }
        
        public IDebugController DebugController { get; set; }
        
        public string CodeStatFile { get; set; }

        public bool CodeStatisticsEnabled => CodeStatFile != null;

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
            CodeStatProcessor codeStatProcessor = null;
            if (CodeStatisticsEnabled)
            {
                codeStatProcessor = new CodeStatProcessor();
                builder.Services.RegisterSingleton<ICodeStatCollector>(codeStatProcessor);
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

            if (codeStatProcessor != null)
            {
                codeStatProcessor.EndCodeStat();
                var codeStat = codeStatProcessor.GetStatData();
                var statsWriter = new CodeStatWriter(CodeStatFile, CodeStatWriterType.JSON);
                statsWriter.Write(codeStat);
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
