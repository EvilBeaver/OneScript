using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine;
using ScriptEngine.HostedScript;

namespace oscript
{
    class ShowCompiledBehavior : AppBehavior
    {
        string _path;

        public ShowCompiledBehavior(string path)
        {
            _path = path;
        }

        public override int Execute()
        {
            var hostedScript = new HostedScriptEngine();
            hostedScript.Initialize();
            var source = hostedScript.Loader.FromFile(_path);
            var compiler = hostedScript.GetCompilerService();
            var writer = new ScriptEngine.Compiler.ModuleWriter(compiler);
            writer.Write(Console.Out, source);
            return 0;
        }
    }
}
