/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using oscript;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace StandaloneRunner
{
    public class StandaloneApplicationHost : IHostApplication
    {
        public string[] CommandLineArguments { get; set; } = new string[0];
        
        public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
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
            return CommandLineArguments;
        }
    }
}