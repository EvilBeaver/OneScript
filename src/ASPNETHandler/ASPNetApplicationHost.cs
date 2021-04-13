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
using System.Threading.Tasks;
using OneScript.StandardLibrary;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace OneScript.ASPNETHandler
{
    class ASPNetApplicationHost : IHostApplication
    {
        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            if (status == MessageStatusEnum.Ordinary)
            {
                Console.WriteLine(str);
            }
            else
            {
                var oldColor = Console.ForegroundColor;
                ConsoleColor newColor;

                switch (status)
                {
                    case MessageStatusEnum.Information:
                        newColor = ConsoleColor.Green;
                        break;
                    case MessageStatusEnum.Attention:
                        newColor = ConsoleColor.Yellow;
                        break;
                    case MessageStatusEnum.Important:
                    case MessageStatusEnum.VeryImportant:
                        newColor = ConsoleColor.Red;
                        break;
                    default:
                        newColor = oldColor;
                        break;
                }

                try
                {
                    Console.ForegroundColor = newColor;
                    Console.WriteLine(str);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        public void ShowExceptionInfo(Exception exc)
        {
            throw new NotImplementedException();
        }

        public bool InputString(out string result, string prompt, int maxLen, bool multiline)
        {
            throw new NotImplementedException();
        }

        public string[] GetCommandLineArguments()
        {
            throw new NotImplementedException();
        }
    }
}
