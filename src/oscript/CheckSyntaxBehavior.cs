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
        bool   _isCgi;

        public CheckSyntaxBehavior(string path, string envFile, bool isCgi = false)
        {
            _path = path;
            _envFile = envFile;
            _isCgi = isCgi;
        } 

        public override int Execute()
        {
            var hostedScript = new HostedScriptEngine();
            hostedScript.CustomConfig = ScriptFileHelper.CustomConfigPath(_path);
            hostedScript.Initialize();

            if (_isCgi) {
                var request = ScriptEngine.Machine.ValueFactory.Create ();
                hostedScript.InjectGlobalProperty ("ВебЗапрос", request, true);
                hostedScript.InjectGlobalProperty ("WebRequest", request, true);
            }

            ScriptFileHelper.OnBeforeScriptRead(hostedScript);
            var source = hostedScript.Loader.FromFile(_path);
            hostedScript.SetGlobalEnvironment(new SimpleConsoleHost(), source);

            try
            {
                if(_envFile != null)
                {
                    var envCompiler = hostedScript.GetCompilerService();
                    var envSrc = hostedScript.Loader.FromFile(_envFile);
                    envCompiler.CreateModule(envSrc);
                }
                var compiler = hostedScript.GetCompilerService();
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
