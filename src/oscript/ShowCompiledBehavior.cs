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
            hostedScript.CustomConfig = ScriptFileHelper.CustomConfigPath(_path);
            hostedScript.Initialize();
            var source = hostedScript.Loader.FromFile(_path);
            var compiler = hostedScript.GetCompilerService();
            var writer = new ScriptEngine.Compiler.ModuleWriter(compiler);
            writer.Write(Console.Out, source);
            return 0;
        }
    }
}
