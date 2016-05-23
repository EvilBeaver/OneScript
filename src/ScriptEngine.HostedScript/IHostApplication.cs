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

namespace ScriptEngine.HostedScript
{
    public interface IHostApplication
    {
        void Echo(string str, EchoStatus status = EchoStatus.Undefined);
        void ShowExceptionInfo(Exception exc);
        bool InputString(out string result, int maxLen);
        string[] GetCommandLineArguments();
    }

    public enum EchoStatus
    {
        Undefined,
        WithoutStatus,
        Ordinary,
        Information,
        Attention,
        Important,
        VeryImportant
    }
}
