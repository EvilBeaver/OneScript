/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.Values;
using OneScript.Web.Server;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace NUnitTests.Bsl
{
    public class BslScriptProcess
    {
        private HostedScriptEngine _engine;
        private UserScriptContextInstance _testRunner;
        private CapturingHostApplication _hostApp;

        public HostedScriptEngine Engine => _engine;
        
        public BslScriptProcess(string runnerPath)
        {
            if (string.IsNullOrEmpty(runnerPath))
                throw new ArgumentException("Не указан путь к testrunner.os", nameof(runnerPath));

            var builder = DefaultEngineBuilder.Create()
                .SetupConfiguration(p =>
                {
                    p.UseSystemConfigFile()
                        .UseEnvironmentVariableConfig("OSCRIPT_CONFIG")
                        .UseEntrypointConfigFile(runnerPath);
                })
                .SetDefaultOptions()
                .UseImports()
                .UseFileSystemLibraries()
                .UseNativeRuntime()
                .UseEventHandlers()
                .SetupEnvironment(env =>
                {
                    env.AddStandardLibrary()
                        .AddWebServer()
                        .UseTemplateFactory(new DefaultTemplatesFactory());
                });
            
            var coreEngine = builder.Build(); 
            _engine = new HostedScriptEngine(coreEngine);
            var src = _engine.Loader.FromFile(runnerPath);
            _hostApp = new CapturingHostApplication();
            _engine.SetGlobalEnvironment(_hostApp, src);
            _engine.Initialize();

            var compiler = _engine.GetCompilerService();
            var bslProcess = _engine.Engine.NewProcess();
            var executable = compiler.Compile(src, bslProcess);
            _testRunner = _engine.Engine.NewObject(executable, bslProcess);
        }

        public BslProcessResult Execute(string methodName, IValue[] args)
        {
            _hostApp.ClearMessages();
            var index = _testRunner.GetMethodNumber(methodName);
            var methodInfo = _testRunner.GetMethodInfo(index);

            var process = _engine.Engine.NewProcess();
            IValue result = null;
            if (methodInfo.IsFunction())
            {
                _testRunner.CallAsFunction(index, args, out result, process);
            }
            else
            {
                _testRunner.CallAsProcedure(index, args, process);
            }

            var processResult = new BslProcessResult((BslValue)result, new List<BslLogMessage>(_hostApp.Messages));
            ClearMessages();

            return processResult;
        }

        internal void ClearMessages()
        {
            _hostApp.ClearMessages();
        }
    }
}