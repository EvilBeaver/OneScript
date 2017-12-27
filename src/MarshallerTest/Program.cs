using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.HostedScript;
using System.Collections.Generic;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;

namespace TestBot
{
    class Program
    {
        static void Main(string[] args)
        {

            /*

            _hostedScript = new HostedScriptEngine();
            _hostedScript.Initialize();
            var runner = _hostedScript.EngineInstance.AttachedScriptsFactory.LoadFromString(
            _hostedScript.EngineInstance.GetCompilerService(), sourceCode);


    */

            var hostedScript = new HostedScriptEngine();
            hostedScript.Initialize();
            System.IO.StreamReader sr = System.IO.File.OpenText("testbot.os");
            string text = sr.ReadToEnd();
            hostedScript.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            var src = hostedScript.Loader.FromString(text);
            Process process = null;
            MyHost myOut = new MyHost();
            process = hostedScript.CreateProcess(myOut, src);
            var returnCode = process.Start();
        }
    }

    // Класс выводящий вывод OneScript на экран
    public class MyHost : IHostApplication
    {
        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            Console.WriteLine(str);
        }
        public void ShowExceptionInfo(Exception exc)
        {
            Console.WriteLine("Exception: " + exc.Message);
        }
        public bool InputString(out string result, int maxLen)
        {
            result = "строка введена";
            return true;
        }
        public string[] GetCommandLineArguments()
        {
            return new string[0];
        }
    }
}
