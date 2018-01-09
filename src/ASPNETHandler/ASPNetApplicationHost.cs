using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool InputString(out string result, int maxLen)
        {
            throw new NotImplementedException();
        }

        public string[] GetCommandLineArguments()
        {
            throw new NotImplementedException();
        }
    }
}
