/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

using ScriptEngine;
using ScriptEngine.HostedScript.Library;

namespace oscript
{
    internal static class ConsoleHostImpl
    {
        public static void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            if (status == MessageStatusEnum.Ordinary)
            {
                Output.WriteLine(text);
            }
            else
            {
                var oldColor = Output.TextColor;
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
                    Output.TextColor = newColor;
                    Output.WriteLine(text);
                }
                finally
                {
                    Output.TextColor = oldColor;
                }
            }
        }

        public static void ShowExceptionInfo(Exception exc)
        {
            if (exc is ScriptException rte)
                Echo(rte.MessageWithoutCodeFragment);
            else
                Echo(exc.Message);
        }

        public static bool InputString(out string result, int maxLen)
        {
            if (maxLen == 0)
                result = Console.ReadLine();
            else
                result = Console.ReadLine()?.Substring(0, maxLen);

            return result?.Length > 0;
        }
    }
}