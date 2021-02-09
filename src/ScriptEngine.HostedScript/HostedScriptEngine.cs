/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.StandardLibrary;
using ScriptEngine.Compiler;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Hosting;


namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine : IDisposable
    {
        private readonly ScriptingEngine _engine;
        private SystemGlobalContext _globalCtx;
        private readonly RuntimeEnvironment _env;
        private bool _isInitialized;
        private bool _configInitialized;
        private bool _librariesInitialized;

        private CodeStatProcessor _codeStat;

        public HostedScriptEngine(ScriptingEngine engine)
        {
            _engine = engine;
            _env = _engine.Environment;
            _engine.AttachAssembly(typeof(HostedScriptEngine).Assembly);
            
            SetGlobalContexts(engine.GlobalsManager);
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

            InitializationCallback = (eng, env) =>
            {
                var templateFactory = new DefaultTemplatesFactory();
                var storage = new TemplateStorage(templateFactory);
                env.InjectObject(storage);
                manager.RegisterInstance(storage);
            };
        }
        
        public ScriptingEngine EngineInstance => _engine;

        public void InitExternalLibraries(string systemLibrary, IEnumerable<string> searchDirs)
        {
            _librariesInitialized = true;
        }

        public KeyValueConfig GetWorkingConfig()
        {
            var cfgAccessor = EngineInstance.GlobalsManager.GetInstance<SystemConfigAccessor>();
            if (!_configInitialized)
            {
                cfgAccessor.Provider = _engine.Configuration;
                cfgAccessor.Refresh();
                _configInitialized = true;
            }
            return cfgAccessor.GetConfig();
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
            var SystemLanguageCfg = GetWorkingConfig()["SystemLanguage"];

            if (SystemLanguageCfg != null)
                Locale.SystemLanguageISOName = SystemLanguageCfg;
            else
                Locale.SystemLanguageISOName = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        private void InitLibraries(KeyValueConfig config)
        {
            if (_librariesInitialized)
                return;

            if(config != null)
            {
                InitLibrariesFromConfig(config);
            }
            else
            {
                InitExternalLibraries(null, null);
            }
        }

        private void InitLibrariesFromConfig(KeyValueConfig config)
        {
            string sysDir = config[OneScriptOptions.SYSTEM_LIBRARY_DIR];
            string additionalDirsList = config[OneScriptOptions.ADDITIONAL_LIBRARIES];
            string[] addDirs = null;
            
            if(additionalDirsList != null)
            {
                addDirs = additionalDirsList.Split(';');
            }

            InitExternalLibraries(sysDir, addDirs);

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
            InitLibraries(GetWorkingConfig());

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

        public Process CreateProcess(IHostApplication host, ICodeSource src)
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
            var definitions = GetWorkingConfig()["preprocessor.define"]?.Split(',') ?? new string[0];
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

        public Process CreateProcess(IHostApplication host, ModuleImage moduleImage, ICodeSource src)
        {
            SetGlobalEnvironment(host, src);
            var module = _engine.LoadModuleImage(moduleImage);
            return InitProcess(host, module);
        }

        public void SetGlobalEnvironment(IHostApplication host, ICodeSource src)
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
