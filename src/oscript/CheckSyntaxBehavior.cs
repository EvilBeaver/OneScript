/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
        string _envFile;

        public CheckSyntaxBehavior(string path, string envFile)
        {
            _path = path;
            _envFile = envFile;
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
                if(_envFile != null)
                {
                    var envSrc = hostedScript.Loader.FromFile(_envFile);
                    compiler.CreateModule(envSrc);
                }
                compiler.CreateModule(source);
            }
            catch (ScriptException e)
            {
                Output.WriteLine(e.Message);
                return 1;
            }

            Output.WriteLine("No errors.");

            return 0;
        }
    }
}
