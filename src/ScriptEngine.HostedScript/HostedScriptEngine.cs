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
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;

using OneScript.DebugProtocol;

namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine
    {
        readonly ScriptingEngine _engine;

        readonly SystemGlobalContext _globalCtx;

        readonly RuntimeEnvironment _env;
        bool _isInitialized;
        bool _configInitialized;

        private CodeStatProcessor _codeStat;

        public HostedScriptEngine()
        {
            _engine = new ScriptingEngine();
            _env = new RuntimeEnvironment();
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly(), _env);

            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx, false);
            _engine.Environment = _env;
        }

        public void InitExternalLibraries(string systemLibrary, IEnumerable<string> searchDirs)
        {
            var libLoader = new LibraryResolver(_engine, _env);
            _engine.DirectiveResolver = libLoader;

            libLoader.LibraryRoot = systemLibrary;
            libLoader.SearchDirectories.Clear();
            if (searchDirs != null)
            {
                libLoader.SearchDirectories.AddRange(searchDirs);
            }
        }

        public static string ConfigFileName
        {
            get
            {
                return EngineConfigProvider.CONFIG_FILE_NAME;
            }
        }

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

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _engine.Initialize();
                TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));
                _isInitialized = true;
            }
        }

        private void InitLibraries(KeyValueConfig config)
        {
            if (_engine.DirectiveResolver != null)
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

        public ICodeSourceFactory Loader
        {
            get
            {
                return _engine.Loader;
            }
        }

        public IDebugController DebugController
        {
            get { return _engine.DebugController; }
            set { _engine.DebugController = value; }
        }

        private void InitializeDirectiveResolver()
        {
            var ignoreDirectiveResolver = new DirectiveIgnorer();

            ignoreDirectiveResolver.Add("Region", "Область");
            ignoreDirectiveResolver.Add("EndRegion", "КонецОбласти");

            var resolversCollection = new DirectiveMultiResolver();
            resolversCollection.Add(ignoreDirectiveResolver);

            if (_engine.DirectiveResolver != null)
                resolversCollection.Add(_engine.DirectiveResolver);

            _engine.DirectiveResolver = resolversCollection;
        }

        public CompilerService GetCompilerService()
        {
            InitLibraries(GetWorkingConfig());
            InitializeDirectiveResolver();

            var compilerSvc = _engine.GetCompilerService();
            compilerSvc.DefineVariable("ЭтотОбъект", SymbolType.ContextProperty);
            return compilerSvc;
        }

        public IEnumerable<UserAddedScript> GetUserAddedScripts()
        {
            return _env.GetUserAddedScripts();
        }

        public void LoadUserScript(UserAddedScript script)
        {
            if (script.Type == UserAddedScriptType.Class)
            {
                _engine.AttachedScriptsFactory.LoadAndRegister(script.Symbol, script.Module);
            }
            else
            {
                var loaded = _engine.LoadModuleImage(script.Module);
                var instance = (IValue)_engine.NewObject(loaded);
                _env.InjectGlobalProperty(instance, script.Symbol, true);
            }
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src)
        {
            return CreateProcess(host, src, GetCompilerService());
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src, CompilerService compilerSvc)
        {
            SetGlobalEnvironment(host, src);
            Initialize();
            _engine.DebugController?.OnMachineReady(_engine.Machine);
            _engine.DebugController?.WaitForDebugEvent(DebugEventType.BeginExecution);
            var module = _engine.LoadModuleImage(compilerSvc.CreateModule(src));
            return InitProcess(host, ref module);
        }

        public Process CreateProcess(IHostApplication host, ScriptModuleHandle moduleHandle, ICodeSource src)
        {
            SetGlobalEnvironment(host, src);
            var module = _engine.LoadModuleImage(moduleHandle);
            return InitProcess(host, ref module);
        }

        public void SetGlobalEnvironment(IHostApplication host, ICodeSource src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        private Process InitProcess(IHostApplication host, ref LoadedModuleHandle module)
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
            _codeStat?.EndCodeStat();
        }
    }
}
