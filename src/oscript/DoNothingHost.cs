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

using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace oscript
{
    class DoNothingHost : IHostApplication
    {
        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            
        }

        public void ShowExceptionInfo(Exception exc)
        {
            
        }

        public bool InputString(out string result, int maxLen)
        {
            result = "";
            return true;
        }

        public string[] GetCommandLineArguments()
        {
            return new[]
            {
                ""
            };
        }
    }
}
