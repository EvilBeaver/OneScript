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
using ScriptEngine.Machine;

namespace oscript
{
    internal class CheckSyntaxBehavior : AppBehavior
    {
        private readonly string _envFile;
        private readonly bool _isCgi;
        private readonly string _path;

        public CheckSyntaxBehavior(string path, string envFile, bool isCgi = false)
        {
            _path = path;
            _envFile = envFile;
            _isCgi = isCgi;
        }

        public override int Execute()
        {
            var hostedScript = new HostedScriptEngine
            {
                CustomConfig = ScriptFileHelper.CustomConfigPath(_path)
            };
            hostedScript.Initialize();

            if (_isCgi)
            {
                var request = ValueFactory.Create();
                hostedScript.InjectGlobalProperty("ВебЗапрос", request, true);
                hostedScript.InjectGlobalProperty("WebRequest", request, true);
            }

            ScriptFileHelper.OnBeforeScriptRead(hostedScript);
            var source = hostedScript.Loader.FromFile(_path);

            hostedScript.SetGlobalEnvironment(new DoNothingHost(), source);

            try
            {
                if (_envFile != null)
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