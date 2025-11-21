/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.Values;
using OneScript.Web.Server;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace BslTestsBridge.BslBridge
{
    public class BslExecutor
    {
        private readonly HostedScriptEngine _engine;
        private readonly UserScriptContextInstance _scriptInstance;
        private readonly CapturingHostApplication _hostApp;

        public HostedScriptEngine Engine => _engine;
        
        public BslExecutor(string runnerPath)
        {
            if (string.IsNullOrEmpty(runnerPath))
                throw new FileNotFoundException(runnerPath);

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
            _scriptInstance = _engine.Engine.NewObject(executable, bslProcess);
        }

        public BslProcessResult Execute(string methodName, IValue[] args)
        {
            _hostApp.ClearMessages();
            var index = _scriptInstance.GetMethodNumber(methodName);
            var methodInfo = _scriptInstance.GetMethodInfo(index);

            var process = _engine.Engine.NewProcess();
            IValue result = null;
            if (methodInfo.IsFunction())
            {
                _scriptInstance.CallAsFunction(index, args, out result, process);
            }
            else
            {
                _scriptInstance.CallAsProcedure(index, args, process);
            }

            var processResult = new BslProcessResult((BslValue)result, new List<BslLogMessage>(_hostApp.Messages));
            ClearMessages();

            return processResult;
        }

        private void ClearMessages()
        {
            _hostApp.ClearMessages();
        }
    }
}