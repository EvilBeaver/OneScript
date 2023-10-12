/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Sources;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Tasks;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine : IDisposable
    {
        private readonly ScriptingEngine _engine;
        private SystemGlobalContext _globalCtx;
        private readonly RuntimeEnvironment _env;
        private bool _isInitialized;

        private readonly OneScriptLibraryOptions _workingConfig;

        public HostedScriptEngine(ScriptingEngine engine)
        {
            _engine = engine;
            _env = _engine.Environment;
            _engine.AttachAssembly(typeof(HostedScriptEngine).Assembly);
            _workingConfig = _engine.Services.Resolve<OneScriptLibraryOptions>();
            SetGlobalContexts(engine.GlobalsManager);
        }

        private void SetGlobalContexts(IGlobalsManager manager)
        {
            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx);
            manager.RegisterInstance(_globalCtx);

            var dynLoader = new DynamicLoadingFunctions(_engine);
            _env.InjectObject(dynLoader);
            manager.RegisterInstance(dynLoader);

            var bgTasksManager = new BackgroundTasksManager(_engine.Services.Resolve<ExecutionContext>());
            _env.InjectGlobalProperty(bgTasksManager, "ФоновыеЗадания", "BackgroundJobs", true);
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _engine.Initialize();
                _isInitialized = true;
            }

            // System language
            var systemLanguageCfg = _workingConfig.SystemLanguage;

            Locale.SystemLanguageISOName = systemLanguageCfg ?? System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        public void InjectGlobalProperty(string name, string alias, IValue value, bool readOnly)
        {
            _env.InjectGlobalProperty(value, name, alias, readOnly);
        }

        public void InjectObject(IAttachableContext obj)
        {
            _env.InjectObject(obj);
        }

        public ScriptSourceFactory Loader => _engine.Loader;

        public ICompilerFrontend GetCompilerService()
        {
            var compilerSvc = _engine.GetCompilerService();
            compilerSvc.FillSymbols(typeof(UserScriptContextInstance));
            
            return compilerSvc;
        }

        public Process CreateProcess(IHostApplication host, SourceCode src)
        {
            Initialize();
            SetGlobalEnvironment(host, src);
            if (_engine.DebugController != null)
            {
                _engine.DebugController.Init();
                _engine.DebugController.AttachToThread();
                _engine.DebugController.Wait();
            }

            var compilerSvc = GetCompilerService();
            DefineConstants(compilerSvc);
            IExecutableModule module;
            try
            {
                module = compilerSvc.Compile(src);
            }
            catch (CompilerException)
            {
                _engine.DebugController?.NotifyProcessExit(1);
                throw;
            }
            return InitProcess(host, module);
        }

        private void DefineConstants(ICompilerFrontend compilerSvc)
        {
            var definitions = _workingConfig.PreprocessorDefinitions;
            foreach (var val in definitions)
            {
                compilerSvc.PreprocessorDefinitions.Add(val);
            }

            if (Utils.IsMonoRuntime)
            {
                compilerSvc.PreprocessorDefinitions.Add("MONO");
            }
        }

        public void SetGlobalEnvironment(IHostApplication host, SourceCode src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        private Process InitProcess(IHostApplication host, IExecutableModule module)
        {
            Initialize();
            
            var process = new Process(host, module, _engine);
            return process;
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
