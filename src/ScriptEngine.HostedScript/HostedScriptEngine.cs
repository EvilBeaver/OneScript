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
using System.Collections.Generic;
using OneScript.Commons;
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
            
            SetGlobalContexts(engine.GlobalsManager);

            _workingConfig = new Lazy<OneScriptLibraryOptions>(() =>
            {
                var cfgAccessor = EngineInstance.GlobalsManager.GetInstance<SystemConfigAccessor>();
                cfgAccessor.Provider = _engine.Configuration;
                cfgAccessor.Refresh();
                
                return new OneScriptLibraryOptions(cfgAccessor.GetConfig());
            });
        }

        private void SetGlobalContexts(IGlobalsManager manager)
        {
            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx, false);
            manager.RegisterInstance(_globalCtx);

            var dynLoader = new DynamicLoadingFunctions(_engine);
            _env.InjectObject(dynLoader, false);
            manager.RegisterInstance(dynLoader);

            var bgTasksManager = new BackgroundTasksManager(_engine.Services.Resolve<MachineEnvironment>());
            _env.InjectGlobalProperty(bgTasksManager, "ФоновыеЗадания", "BackgroundJobs", true);
        }
        
        public ScriptingEngine EngineInstance => _engine;

        private OneScriptLibraryOptions GetWorkingConfig()
        {
            return _workingConfig.Value;
        }

        public Action<ScriptingEngine, RuntimeEnvironment> InitializationCallback { get; set; }
        
        public void Initialize()
        {
            if (!_isInitialized)
            {
                InitializationCallback?.Invoke(_engine, _engine.Environment);
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

        public void InjectObject(IAttachableContext obj, bool asDynamicScope)
        {
            _env.InjectObject(obj, asDynamicScope);
        }

        public ScriptSourceFactory Loader => _engine.Loader;

        public IDebugController DebugController
        {
            get => _engine.DebugController;
            set => _engine.DebugController = value;
        }

        public ICompilerService GetCompilerService()
        {
            var compilerSvc = _engine.GetCompilerService();
            UserScriptContextInstance.PrepareCompilation(compilerSvc);
            
            return compilerSvc;
        }

        public IEnumerable<ExternalLibraryDef> GetExternalLibraries()
        {
            return _env.GetUserAddedScripts();
        }

        public void LoadUserScript(UserAddedScript script)
        {
            if (script.Type == UserAddedScriptType.Class)
            {
                _engine.AttachedScriptsFactory.LoadAndRegister(script.Symbol, script.Image);
            }
            else
            {
                var loaded = _engine.LoadModuleImage(script.Image);
                var instance = (IValue)_engine.NewObject(loaded);
                _env.InjectGlobalProperty(instance, script.Symbol, true);
            }
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
            LoadedModule module;
            try
            {
                var image = compilerSvc.Compile(src);
                module = _engine.LoadModuleImage(image);
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

            if (Utils.IsNetCore)
            {
                compilerSvc.DefinePreprocessorValue("NETCORE");
            }

            if (Utils.IsNetFramework)
            {
                compilerSvc.DefinePreprocessorValue("NETFRAMEWORK");
            }

            if (Utils.IsMonoRuntime)
            {
                compilerSvc.DefinePreprocessorValue("MONO");
            }
        }

        public Process CreateProcess(IHostApplication host, ModuleImage moduleImage, SourceCode src)
        {
            SetGlobalEnvironment(host, src);
            var module = _engine.LoadModuleImage(moduleImage);
            return InitProcess(host, module);
        }

        public void SetGlobalEnvironment(IHostApplication host, SourceCode src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        private Process InitProcess(IHostApplication host, LoadedModule module)
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
