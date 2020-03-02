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
using OneScript.StandardLibrary;
using ScriptEngine.Machine.Contexts;
using OneScript.StandardLibrary.Collections;


namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine : IDisposable
    {
        private readonly ScriptingEngine _engine;
        private readonly SystemGlobalContext _globalCtx;
        private readonly RuntimeEnvironment _env;
        private bool _isInitialized;
        private bool _configInitialized;
        private bool _librariesInitialized;

        private CodeStatProcessor _codeStat;

        public HostedScriptEngine()
        {
            _engine = new ScriptingEngine();
            _env = new RuntimeEnvironment();
            _engine.AttachAssembly(typeof(ArrayImpl).Assembly, _env);
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly(), _env);

            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx, false);
            GlobalsManager.RegisterInstance(_globalCtx);

            InitializationCallback = (eng, env) =>
            {
                var templateFactory = new DefaultTemplatesFactory();
                var storage = new TemplateStorage(templateFactory);
                env.InjectObject(storage);
                GlobalsManager.RegisterInstance(storage);
            };

            _engine.Environment = _env;
        }

        public ScriptingEngine EngineInstance => _engine;

        public void InitExternalLibraries(string systemLibrary, IEnumerable<string> searchDirs)
        {
            var libLoader = new LibraryResolver(_engine, _env);
            _engine.DirectiveResolvers.Add(libLoader);

            libLoader.LibraryRoot = systemLibrary;
            libLoader.SearchDirectories.Clear();
            if (searchDirs != null)
            {
                libLoader.SearchDirectories.AddRange(searchDirs);
            }

            _librariesInitialized = true;
        }

        public static string ConfigFileName => EngineConfigProvider.CONFIG_FILE_NAME;

        public KeyValueConfig GetWorkingConfig()
        {
            var cfgAccessor = GlobalsManager.GetGlobalContext<SystemConfigAccessor>();
            if (!_configInitialized)
            {
                cfgAccessor.Provider = new EngineConfigProvider(CustomConfig);
                cfgAccessor.Refresh();
                _configInitialized = true;
            }
            return cfgAccessor.GetConfig();
        }

        public string CustomConfig { get; set; }

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

        private static string ConfigFilePath()
        {
            return EngineConfigProvider.DefaultConfigFilePath();
        }

        private void InitLibrariesFromConfig(KeyValueConfig config)
        {
            string sysDir = config[EngineConfigProvider.SYSTEM_LIB_KEY];
            string additionalDirsList = config[EngineConfigProvider.ADDITIONAL_LIB_KEY];
            string[] addDirs = null;
            
            if(additionalDirsList != null)
            {
                addDirs = additionalDirsList.Split(';');
            }

            InitExternalLibraries(sysDir, addDirs);

        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            _engine.AttachAssembly(asm, _env);
        }

        public void InjectGlobalProperty(string name, IValue value, bool readOnly)
        {
            _env.InjectGlobalProperty(value, name, readOnly);
        }

        public void InjectObject(IAttachableContext obj, bool asDynamicScope)
        {
            _env.InjectObject(obj, asDynamicScope);
        }

        public ICodeSourceFactory Loader => _engine.Loader;

        public IDebugController DebugController
        {
            get => _engine.DebugController;
            set => _engine.DebugController = value;
        }

        public CompilerService GetCompilerService()
        {
            InitLibraries(GetWorkingConfig());

            var compilerSvc = _engine.GetCompilerService();
            compilerSvc.DefineVariable("ЭтотОбъект", "ThisObject", SymbolType.ContextProperty);
            UserScriptContextInstance.GetOwnMethodsDefinition().ForEach(x => compilerSvc.DefineMethod(x));
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
                _engine.DebugController.AttachToThread(_engine.Machine);
                _engine.DebugController.Wait();
            }

            var compilerSvc = GetCompilerService();
            DefineConstants(compilerSvc);
            var module = _engine.LoadModuleImage(compilerSvc.Compile(src));
            return InitProcess(host, module);
        }

        private void DefineConstants(CompilerService compilerSvc)
        {
            var definitions = GetWorkingConfig()["preprocessor.define"]?.Split(',') ?? new string[0];
            foreach (var val in definitions)
            {
                compilerSvc.DefinePreprocessorValue(val);
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
