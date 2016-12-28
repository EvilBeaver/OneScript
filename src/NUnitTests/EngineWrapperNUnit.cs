using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using System.IO;

namespace NUnitTests
{
    class EngineWrapperNUnit : IHostApplication
    {

        private HostedScriptEngine engine;
        private string[] commandLineArgs;

        public HostedScriptEngine StartEngine()
        {
            engine = new HostedScriptEngine();
            engine.Initialize();

            commandLineArgs = new string[] { };

            return engine;
        }

        private void RunTestScript(ICodeSource source, string resourceName)
        {
            var module = engine.GetCompilerService().CreateModule(source);

            engine.LoadUserScript(new ScriptEngine.UserAddedScript()
            {
                Type = ScriptEngine.UserAddedScriptType.Class,
                Module = module,
                Symbol = resourceName
            });

            var process = engine.CreateProcess(this, source);
            process.Start();

        }

        internal void RunTestScriptFromPath(string scriptFilePath, String argsScript = "")
        {
            string codeSource;

            using (Stream s = File.OpenRead(scriptFilePath))
            {
                using (StreamReader r = new StreamReader(s))
                {
                    codeSource = r.ReadToEnd();
                }
            }

            if (argsScript != "")
            {
                commandLineArgs = argsScript.Split(' ');
            }

            ICodeSource sourceToCompile = engine.Loader.FromString(codeSource);

            RunTestScript(sourceToCompile, scriptFilePath);
        }

        public EngineWrapperNUnit()
        {
        }
        public string[] GetCommandLineArguments()
        {
            return this.commandLineArgs;
        }

        public bool InputString(out string result, int maxLen)
        {
            result = "";
            return false;
        }

        public void ShowExceptionInfo(Exception exc)
        {
            throw exc;
        }

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            Console.WriteLine(str);
        }

        public HostedScriptEngine Engine
        {
            get
            {
                return engine;
            }
        }
 
    }
}
