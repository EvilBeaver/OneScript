using ScriptEngine;
using ScriptEngine.HostedScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    static class ConsoleHostImpl
    {
        public static void Echo(string text, EchoStatus status = EchoStatus.Undefined)
        {
            if (status == EchoStatus.Undefined || status == EchoStatus.Ordinary)
                Output.WriteLine(text);
            else
            {
                ConsoleColor oldColor = Output.TextColor;
                ConsoleColor newColor;

                switch (status)
                {
                    case EchoStatus.Information:
                        newColor = ConsoleColor.Green;
                        break;
                    case EchoStatus.Attention:
                        newColor = ConsoleColor.Yellow;
                        break;
                    case EchoStatus.Important:
                    case EchoStatus.VeryImportant:
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
            if (exc is ScriptException)
            {
                var rte = (ScriptException)exc;
                Echo(rte.MessageWithoutCodeFragment);
            }
            else
                Echo(exc.Message);
        }

        public static bool InputString(out string result, int maxLen)
        {
            if (maxLen == 0)
            {
                result = Console.ReadLine();
            }
            else
            {
                result = Console.ReadLine().Substring(0, maxLen);
            }

            return result.Length > 0;

        }
    }
}
