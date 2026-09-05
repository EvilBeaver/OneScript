/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Sources;
using OneScript.StandardLibrary;
using ScriptEngine;
using ScriptEngine.HostedScript;

namespace oscript
{
    internal class ConsoleApplicationHost(string[] args) : IHostApplication, ISystemLogWriter
    {
        public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
            => ConsoleHostImpl.Echo(text, status);

        public void ShowExceptionInfo(Exception exc)
            => ConsoleHostImpl.ShowExceptionInfo(exc);

        public bool InputString(out string result, string prompt, int maxLen, bool multiline)
            => ConsoleHostImpl.InputString(out result, prompt, maxLen, multiline);

        public string[] GetCommandLineArguments()
            => args;

        public void Write(string text)
            => Console.Error.WriteLine(text);

        public int RunProcess(HostedScriptEngine engine, SourceCode source)
        {
            SystemLogger.SetWriter(this);
            return engine.RunProcess(this, source);
        }
    }
}
