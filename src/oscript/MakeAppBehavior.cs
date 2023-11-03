/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Compilation;
using OneScript.Execution;
using OneScript.Language;
using ScriptEngine;

namespace oscript
{
    internal class MakeAppBehavior : AppBehavior
    {
        private readonly string _inputFile; 
        private readonly string _outputFile;

        private MakeAppBehavior(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
        }

        public override int Execute()
        {
            var builder = ConsoleHostBuilder.Create(_inputFile);
            
            var hostedScript = ConsoleHostBuilder.Build(builder);
            hostedScript.Initialize();
			
            var source = hostedScript.Loader.FromFile(_inputFile);
            var compiler = hostedScript.GetCompilerService();
            hostedScript.SetGlobalEnvironment(new DoNothingHost(), source);
            var env = hostedScript.Services.Resolve<RuntimeEnvironment>();
            
            // выбросит исключение, его поймает Program
            var entryModule = compiler.Compile(source);

            var bundle = BundleApp(entryModule, env);

            return 0;
        }

        private object BundleApp(IExecutableModule entryModule, RuntimeEnvironment env)
        {
            throw new NotImplementedException();
        }

        public static AppBehavior Create(CmdLineHelper arg)
        {
            var fileName = arg.Next();
            var outputName = arg.Next();
            if (fileName == default || outputName == default)
            {
                return null;
            }

            return new MakeAppBehavior(fileName, outputName);
        }
    }
}