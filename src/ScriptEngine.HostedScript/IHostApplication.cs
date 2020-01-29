/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.StandardLibrary;
using ScriptEngine.HostedScript.Library;

namespace ScriptEngine.HostedScript
{
    public interface IHostApplication
    {
		void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary);
        void ShowExceptionInfo(Exception exc);
        bool InputString(out string result, int maxLen);
        string[] GetCommandLineArguments();
    }
}
