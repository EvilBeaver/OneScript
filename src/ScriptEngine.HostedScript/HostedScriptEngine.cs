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
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Tasks;
using ScriptEngine.Compiler;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine : IDisposable
    {
        private readonly ScriptingEngine _engine;
        private SystemGlobalContext _globalCtx;
        private readonly RuntimeEnvironment _env;
        private bool _isInitialized;

        private readonly Lazy<OneScriptLibraryOptions> _workingConfig;
        
        private CodeStatProcessor _codeStat;

        public HostedScriptEngine(ScriptingEngine engine)
        {
            _engine = engine;
            _env = _engine.Environment;
            _engine.AttachAssembly(typeof(HostedScriptEngine).Assembly);
            _workingConfig = new Lazy<OneScriptLibraryOptions>(InitWorkingConfig);
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

        private OneScriptLibraryOptions GetWorkingConfig()
        {
            return _workingConfig.Value;
        }
        
        private OneScriptLibraryOptions InitWorkingConfig()
        {
            var cfgAccessor = _engine.GlobalsManager.GetInstance<SystemConfigAccessor>();
            cfgAccessor.Refresh();
                
            return new OneScriptLibraryOptions(cfgAccessor.GetConfig());
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _engine.Initialize();
                _isInitialized = true;
            }

            // System language
            var SystemLanguageCfg = GetWorkingConfig().SystemLanguage;

            if (SystemLanguageCfg != null)
                Locale.SystemLanguageISOName = SystemLanguageCfg;
            else
                Locale.SystemLanguageISOName = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
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

        public ICompilerService GetCompilerService()
        {
            var compilerSvc = _engine.GetCompilerService();
            UserScriptContextInstance.PrepareCompilation(compilerSvc);
            
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
            StackRuntimeModule module;
            try
            {
                module = compilerSvc.CompileStack(src);
            }
            catch (CompilerException)
            {
                _engine.DebugController?.NotifyProcessExit(1);
                throw;
            }
            return InitProcess(host, module);
        }

        private void DefineConstants(ICompilerService compilerSvc)
        {
            var definitions = GetWorkingConfig().PreprocessorDefinitions;
            foreach (var val in definitions)
            {
                compilerSvc.DefinePreprocessorValue(val);
            }

            if (Utils.IsMonoRuntime)
            {
                compilerSvc.DefinePreprocessorValue("MONO");
            }
        }

        public void SetGlobalEnvironment(IHostApplication host, SourceCode src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        private Process InitProcess(IHostApplication host, StackRuntimeModule module)
        {
            Initialize();
            
            var process = new Process(host, module, _engine);
            return process;
        }
        
        public void EnableCodeStatistics()
        {
            _codeStat = new CodeStatProcessor();
            _engine.SetCodeStatisticsCollector(_codeStat);
        }

        public CodeStatDataCollection GetCodeStatData()
        {
            return _codeStat.GetStatData();
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _codeStat?.EndCodeStat();
        }
    }
}
