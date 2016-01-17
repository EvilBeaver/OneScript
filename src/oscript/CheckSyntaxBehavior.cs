using ScriptEngine;
using ScriptEngine.HostedScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class CheckSyntaxBehavior : AppBehavior
    {
        string _path;

        public CheckSyntaxBehavior(string path)
        {
            _path = path;
        } 

        public override int Execute()
        {
            var hostedScript = new HostedScriptEngine();
            hostedScript.CustomConfig = ScriptFileHelper.CustomConfigPath(_path);
            hostedScript.Initialize();
            ScriptFileHelper.OnBeforeScriptRead(hostedScript);
            var source = hostedScript.Loader.FromFile(_path);
            var compiler = hostedScript.GetCompilerService();

            try
            {
                compiler.CreateModule(source);
            }
            catch (Exception e)
            {
                Output.WriteLine(e.Message);
                return 1;
            }

            Output.WriteLine("No errors.");

            return 0;
        }
    }
}
